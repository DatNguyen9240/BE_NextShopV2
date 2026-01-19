using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.Interfaces;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks; 

namespace NextShopV2.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly IInventoryService _inventoryService;
        private readonly ICouponService _couponService;
        private readonly IUserRepository _userRepo;

        public OrderService(IOrderRepository orderRepo, IProductVariantRepository variantRepo, IInventoryService inventoryService, ICouponService couponService, IUserRepository userRepository)
        {
            _orderRepo = orderRepo;
            _variantRepo = variantRepo;
            _inventoryService = inventoryService;
            _couponService = couponService;
            _userRepo = userRepository;
        }

        public async Task<List<OrderResponse>> GetAllAsync()
        {
            var orders = await _orderRepo.GetAllAsync();
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<OrderResponse?> GetByIdAsync(Guid id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            return order.IsNull() ? null : MapToResponse(order!);
        }

        public async Task<List<OrderResponse>> GetByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepo.GetByUserIdAsync(userId);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<(List<OrderResponse> Items, int Total)> GetByUserIdPagedAsync(Guid userId, int page, int pageSize, string? status = null)
        {
            var (items, total) = await _orderRepo.GetByUserIdPagedAsync(userId, page, pageSize, status);
            var mapped = items.Select(MapToResponse).ToList();
            return (mapped, total);
        }

        public async Task<List<OrderResponse>> GetByStatusAsync(string status)
        {
            var orders = await _orderRepo.GetByStatusAsync(status);
            return orders.Select(MapToResponse).ToList();
        }

        private const decimal DefaultTaxRate = 0.10m; // 10% default tax rate

        public async Task<OrderResponse> CreateAsync(Guid userId, CreateOrderRequest request)
        {
            // Validate request has items
            if (request.Items.IsNullOrEmpty())
                throw new ArgumentException("Order must contain at least one item");

            // Generate order ID first
            var orderId = Guid.NewGuid();
            
            // Validate all variants exist and calculate total
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;
            var inventoryChanges = new List<(Guid VariantId, int Quantity)>();

            foreach (var itemRequest in request.Items)
            {
                var variant = await _variantRepo.GetByIdAsync(itemRequest.VariantId);
                if (variant.IsNull())
                    throw new ArgumentException($"Variant {itemRequest.VariantId} not found");

                // Ensure variant and its product are active before allowing ordering
                if (!variant!.IsActive)
                    throw new ArgumentException($"Variant {itemRequest.VariantId} is inactive");
                if (variant.Product != null && !variant.Product.IsActive)
                    throw new ArgumentException($"Product {variant.Product.ProductId} is inactive");

                if (variant.StockQuantity < itemRequest.Quantity)
                    // throw new ArgumentException($"Insufficient stock for variant {itemRequest.VariantId}");
                    Console.WriteLine($"Warning: Insufficient stock for variant {itemRequest.VariantId}, but proceeding for testing");

                var unitPrice = variant.PriceAfterDiscount;
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    VariantId = itemRequest.VariantId,
                    ProductId = variant.Product?.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = unitPrice,
                    Variant = variant,
                    VariantSku = variant.SKU,
                    ProductName = variant.Product?.Name,
                    ProductSku = variant.SKU,
                    VariantOptionsJson = JsonSerializer.Serialize(new { color = variant.Color, size = variant.Size, imageUrl = variant.ImageUrl }),
                    DiscountAmount = 0m,
                    TaxRate = DefaultTaxRate,
                    TaxAmount = Math.Round(unitPrice * itemRequest.Quantity * DefaultTaxRate, 0),
                    TotalAmount = Math.Round(unitPrice * itemRequest.Quantity - 0m + Math.Round(unitPrice * itemRequest.Quantity * DefaultTaxRate, 0), 0)
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.Quantity * orderItem.UnitPrice;

                // Create inventory transaction for stock reduction
                await _inventoryService.UpdateInventoryAsync(
                    itemRequest.VariantId,
                    -itemRequest.Quantity, // Negative value to reduce stock
                    $"Order #{orderId}",
                    "System"
                );
                inventoryChanges.Add((itemRequest.VariantId, itemRequest.Quantity));
            }

            // Apply coupons (if any) and compute total discount amount
            var orderCoupons = new List<OrderCoupon>();
            decimal discountAmount = 0m;
            if (request.CouponIds != null && request.CouponIds.Any())
            {
                foreach (var couponId in request.CouponIds)
                {
                    var coupon = await _couponService.GetByIdAsync(couponId);
                    if (coupon == null || !coupon.IsValid) continue;

                    // Check reservation availability
                    var canReserve = await _couponService.CanReserveCouponAsync(coupon.CouponId);
                    if (!canReserve) continue;

                    var couponDiscount = await _couponService.CalculateDiscountAsync(coupon.Code, totalAmount - discountAmount);
                    if (couponDiscount > 0)
                    {
                        discountAmount += couponDiscount;
                        orderCoupons.Add(new OrderCoupon
                        {
                            OrderId = orderId,
                            CouponId = coupon.CouponId,
                            DiscountAmount = couponDiscount,
                            Status = "Reserved",
                            ReservedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // Distribute coupon discount amount across items proportionally to their line total
            if (discountAmount > 0 && totalAmount > 0)
            {
                var remaining = discountAmount;
                for (int i = 0; i < orderItems.Count; i++)
                {
                    var item = orderItems[i];
                    var lineTotal = item.Quantity * item.UnitPrice;
                    var share = lineTotal / totalAmount;
                    var itemDiscount = (decimal)Math.Round(discountAmount * share, 0);
                    // assign remainder to last item to avoid rounding gaps
                    if (i == orderItems.Count - 1) itemDiscount = remaining;
                    else remaining -= itemDiscount;

                    item.DiscountAmount = itemDiscount;
                    item.TotalAmount = Math.Round(lineTotal - item.DiscountAmount + item.TaxAmount, 0);
                }
            }
            else
            {
                // Ensure TotalAmount fields are set (tax already computed earlier)
                foreach (var it in orderItems)
                {
                    it.TotalAmount = Math.Round(it.Quantity * it.UnitPrice - it.DiscountAmount + it.TaxAmount, 2);
                }
            }

                var finalAmount = Math.Max(0, orderItems.Sum(i => i.TotalAmount) - discountAmount);

                // Fetch user info (name, phone) and default address from DB instead of taking from request
                var user = _userRepo.GetById(userId);
                if (user == null)
                    throw new ArgumentException($"User {userId} not found");

                var buyerName = user.FullName;
                var buyerPhone = user.Phone;
                string? shippingAddress = null;
                var defaultAddress = user.Addresses?.FirstOrDefault(a => a.IsDefault) ?? user.Addresses?.FirstOrDefault();
                if (defaultAddress != null)
                    shippingAddress = defaultAddress.FullAddress;

                // Validate required profile info
                if (string.IsNullOrWhiteSpace(buyerPhone))
                    throw new ArgumentException("Vui lòng cập nhật số điện thoại trong hồ sơ trước khi đặt hàng");
                if (string.IsNullOrWhiteSpace(shippingAddress))
                    throw new ArgumentException("Vui lòng cập nhật địa chỉ giao hàng mặc định trong hồ sơ trước khi đặt hàng");

                var order = new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    SubTotal = totalAmount,
                    DiscountAmount = discountAmount,
                    TotalAmount = finalAmount,
                    BuyerName = buyerName,
                    BuyerPhone = buyerPhone,
                    ShippingAddress = shippingAddress,
                    Items = orderItems,
                    OrderCoupons = orderCoupons,
                    // Không còn CouponId, coupon
                };

                // If payment method is COD, create a pending Payment record so collection can be tracked
                if (!string.IsNullOrEmpty(request.PaymentMethod) && request.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    order.Payments.Add(new NextShopV2.Domain.Entities.Payments.Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        OrderId = orderId,
                        Method = "COD",
                        Amount = finalAmount,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    });
                }

            try
            {
                await _orderRepo.AddAsync(order);
                await _orderRepo.SaveAsync();
            }
            catch
            {
                // Rollback inventory changes
                foreach (var change in inventoryChanges)
                {
                    try
                    {
                        await _inventoryService.UpdateInventoryAsync(
                            change.VariantId,
                            change.Quantity, // add back
                            $"Rollback for order #{orderId}",
                            "System"
                        );
                    }
                    catch
                    {
                        // swallow rollback errors to avoid masking original exception
                    }
                }
                throw;
            }

            return MapToResponse(order);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order == null) return false;

            var previous = order.Status; 
            order.Status = request.Status;
            await _orderRepo.UpdateAsync(order);
            await _orderRepo.SaveAsync();

            // Notify user if status is one of interest
            var notifyStatuses = new[] { "Paid", "Shipped", "Completed", "Cancelled" };
            if (!string.Equals(previous, order.Status, StringComparison.OrdinalIgnoreCase) && Array.Exists(notifyStatuses, s => s.Equals(order.Status, StringComparison.OrdinalIgnoreCase)))
            {
                // Push notification removed
            }

            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order.IsNull()) return false;

            if (order!.Status == "Completed" || order.Status == "Shipped")
                throw new InvalidOperationException("Cannot cancel completed or shipped orders");

            // Restore stock using inventory service (creates transaction)
            foreach (var item in order.Items)
            {
                // If variant was deleted or null, we cannot restore by variant id
                if (item.VariantId.HasValue)
                {
                    await _inventoryService.UpdateInventoryAsync(
                        item.VariantId.Value,
                        item.Quantity,
                        $"Order #{id} cancelled",
                        "System"
                    );
                }
            }

            // Release any coupon reservations
            try
            {
                await _couponService.ReleaseCouponReservationsForOrderAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to release coupon reservations for order {id}: {ex.Message}");
            }

            order.Status = "Cancelled";
            await _orderRepo.UpdateAsync(order);
            await _orderRepo.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _orderRepo.ExistsAsync(id)) return false;

            await _orderRepo.DeleteAsync(id);
            await _orderRepo.SaveAsync();

            return true;
        }

        public async Task<decimal> CalculateOrderTotalAsync(CreateOrderRequest request)
        {
            decimal total = 0;

            foreach (var itemRequest in request.Items)
            {
                var variant = await _variantRepo.GetByIdAsync(itemRequest.VariantId);
                if (variant != null)
                {
                    if (!variant.IsActive || (variant.Product != null && !variant.Product.IsActive))
                        throw new ArgumentException($"Variant {itemRequest.VariantId} or its product is inactive");

                    total += variant.PriceAfterDiscount * itemRequest.Quantity;
                }
            }

            return total;
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                BuyerName = order.BuyerName,
                BuyerPhone = order.BuyerPhone,
                ShippingAddress = order.ShippingAddress,
                Items = order.Items.Select(item =>
                {
                    var variant = item.Variant;
                    return new OrderItemResponse
                    {
                        OrderItemId = item.OrderItemId,
                        VariantId = item.VariantId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = item.DiscountAmount,
                        TaxAmount = item.TaxAmount,
                        TotalAmount = item.TotalAmount,
                        Variant = variant == null ? null : new ProductVariantResponse
                        {
                            ProductVariantId = variant.VariantId,
                            Color = variant.Color,
                            Size = variant.Size,
                            BasePrice = variant.BasePrice,
                            DiscountPercent = variant.DiscountPercent,
                            DiscountAmount = variant.DiscountAmount,
                            PriceAfterDiscount = variant.PriceAfterDiscount,
                            StockQuantity = variant.StockQuantity,
                            IsDefault = variant.IsDefault,
                            DisplayOrder = variant.DisplayOrder,
                            ImageUrl = variant.ImageUrl,
                            ImgHover = string.IsNullOrEmpty(variant.ImgHover) ? variant.ImageUrl : variant.ImgHover
                        },
                        // Gán tên sản phẩm từ relation Variant -> Product nếu có, ưu tiên snapshot
                        ProductName = item.ProductName ?? variant?.Product?.Name,
                        ProductSku = item.ProductSku,
                        VariantSku = item.VariantSku,
                        VariantOptionsJson = item.VariantOptionsJson
                    };
                }).ToList(),
                Coupons = order.OrderCoupons?.Select(oc => new OrderCouponResponse
                {
                    CouponId = oc.CouponId,
                    Code = oc.Coupon?.Code ?? string.Empty,
                    DiscountAmount = oc.DiscountAmount,
                    AppliedAt = oc.AppliedAt
                }).ToList() ?? new List<OrderCouponResponse>(),
                Shipment = order.Shipment == null ? null : new ShipmentResponse
                {
                    ShipmentId = order.Shipment.ShipmentId,
                    OrderId = order.Shipment.OrderId,
                    Carrier = order.Shipment.Carrier,
                    TrackingNumber = order.Shipment.TrackingNumber,
                    Status = order.Shipment.Status,
                    CreatedAt = order.Shipment.CreatedAt
                }
            };
        }
    }
}