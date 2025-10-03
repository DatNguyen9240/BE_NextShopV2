using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
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

        public OrderService(IOrderRepository orderRepo, IProductVariantRepository variantRepo, IInventoryService inventoryService, ICouponService couponService)
        {
            _orderRepo = orderRepo;
            _variantRepo = variantRepo;
            _inventoryService = inventoryService;
            _couponService = couponService;
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

            foreach (var itemRequest in request.Items)
            {
                var variant = await _variantRepo.GetByIdAsync(itemRequest.VariantId);
                if (variant.IsNull())
                    throw new ArgumentException($"Variant {itemRequest.VariantId} not found");

                if (variant!.StockQuantity < itemRequest.Quantity)
                    throw new ArgumentException($"Insufficient stock for variant {itemRequest.VariantId}");

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

                var finalAmount = totalAmount - discountAmount;

                var order = new Order
                {
                    OrderId = orderId,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    SubTotal = totalAmount,
                    DiscountAmount = discountAmount,
                    TotalAmount = finalAmount,
                    ShippingAddress = request.ShippingAddress,
                    Items = orderItems,
                    // Không còn CouponId, coupon
                };

            await _orderRepo.AddAsync(order);
            await _orderRepo.SaveAsync();

            return MapToResponse(order);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order.IsNull()) return false;

            order!.Status = request.Status;
            await _orderRepo.UpdateAsync(order);
            await _orderRepo.SaveAsync();

            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid id)
        {
            var order = await _orderRepo.GetByIdAsync(id);
            if (order.IsNull()) return false;

            if (order!.Status == "Completed" || order.Status == "Shipped")
                throw new InvalidOperationException("Cannot cancel completed or shipped orders");

            // Restore stock
            foreach (var item in order.Items)
            {
                var variant = await _variantRepo.GetByIdAsync(item.VariantId);
                if (variant != null)
                {
                    variant.StockQuantity += item.Quantity;
                    await _variantRepo.UpdateAsync(variant);
                }
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
                        ProductId = item.Variant.ProductId,
                        Color = item.Variant.Color,
                        Size = item.Variant.Size,
                        BasePrice = item.Variant.BasePrice,
                        DiscountPercent = item.Variant.DiscountPercent,
                        DiscountAmount = item.Variant.DiscountAmount,
                        PriceAfterDiscount = item.Variant.PriceAfterDiscount,
                        StockQuantity = item.Variant.StockQuantity,
                        IsDefault = item.Variant.IsDefault,
                        DisplayOrder = item.Variant.DisplayOrder,
                        ImageUrl = item.Variant.ImageUrl
                    }
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