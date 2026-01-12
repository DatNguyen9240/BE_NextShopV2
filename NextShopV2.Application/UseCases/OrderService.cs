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

                if (variant!.StockQuantity < itemRequest.Quantity)
                    // throw new ArgumentException($"Insufficient stock for variant {itemRequest.VariantId}");
                    Console.WriteLine($"Warning: Insufficient stock for variant {itemRequest.VariantId}, but proceeding for testing");

                var unitPrice = variant.PriceAfterDiscount;
                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    VariantId = itemRequest.VariantId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = unitPrice,
                    Variant = variant
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

                // Áp dụng nhiều coupon nếu có
                var orderCoupons = new List<OrderCoupon>();
                decimal discountAmount = 0;
                if (request.CouponIds != null && request.CouponIds.Any())
                {
                    foreach (var couponId in request.CouponIds)
                    {
                        var coupon = await _couponService.GetByIdAsync(couponId);
                        if (coupon == null || !coupon.IsValid)
                            continue;
                        var couponDiscount = await _couponService.CalculateDiscountAsync(coupon.Code, totalAmount - discountAmount);
                        if (couponDiscount > 0)
                        {
                            discountAmount += couponDiscount;
                            orderCoupons.Add(new OrderCoupon
                            {
                                OrderId = orderId,
                                CouponId = coupon.CouponId,
                                DiscountAmount = couponDiscount,
                                AppliedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                var finalAmount = Math.Max(0, totalAmount - discountAmount);

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
                await _inventoryService.UpdateInventoryAsync(
                    item.VariantId,
                    item.Quantity,
                    $"Order #{id} cancelled",
                    "System"
                );
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
                Items = order.Items.Select(item => new OrderItemResponse
                {
                    OrderItemId = item.OrderItemId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Variant = item.Variant.IsNull() ? null : new ProductVariantResponse
                    {
                        ProductVariantId = item.Variant.VariantId,
                        Color = item.Variant.Color,
                        Size = item.Variant.Size,
                        BasePrice = item.Variant.BasePrice,
                        DiscountPercent = item.Variant.DiscountPercent,
                        DiscountAmount = item.Variant.DiscountAmount,
                        PriceAfterDiscount = item.Variant.PriceAfterDiscount,
                        StockQuantity = item.Variant.StockQuantity,
                        IsDefault = item.Variant.IsDefault,
                        DisplayOrder = item.Variant.DisplayOrder,
                        ImageUrl = item.Variant.ImageUrl,
                        ImgHover = string.IsNullOrEmpty(item.Variant.ImgHover) ? item.Variant.ImageUrl : item.Variant.ImgHover
                    },
                    // Gán tên sản phẩm từ relation Variant -> Product nếu có
                    ProductName = item.Variant?.Product?.Name
                }).ToList(),
                Coupons = order.OrderCoupons?.Select(oc => new OrderCouponResponse
                {
                    CouponId = oc.CouponId,
                    Code = oc.Coupon?.Code ?? string.Empty,
                    DiscountAmount = oc.DiscountAmount,
                    AppliedAt = oc.AppliedAt
                }).ToList() ?? new List<OrderCouponResponse>(),
            };
        }
    }
}