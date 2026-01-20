using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.Interfaces;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Request.UpdateDto;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Application.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IShipmentRepository _shipmentRepo;
        private readonly ITrackingService _trackingService;
        private readonly IOrderRepository _orderRepo;
        private readonly IAuthService _authService;

        public ShipmentService(IShipmentRepository shipmentRepo, ITrackingService trackingService, IOrderRepository orderRepo, IAuthService authService)
        {
            _shipmentRepo = shipmentRepo;
            _trackingService = trackingService;
            _orderRepo = orderRepo;
            _authService = authService;
        }

        public async Task<List<ShipmentResponse>> GetAllAsync()
        {
            var shipments = await _shipmentRepo.GetAllAsync();
            var responses = new List<ShipmentResponse>();
            foreach (var shipment in shipments)
            {
                responses.Add(await MapToResponse(shipment));
            }
            return responses;
        }

        public async Task<ShipmentResponse?> GetByIdAsync(Guid id)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(id);
            return shipment.IsNull() ? null : await MapToResponseWithTracking(shipment!);
        }

        public async Task<ShipmentResponse?> GetByOrderIdAsync(Guid orderId)
        {
            var shipment = await _shipmentRepo.GetByOrderIdAsync(orderId);
            return shipment.IsNull() ? null : await MapToResponseWithTracking(shipment!);
        }

        public async Task<List<ShipmentResponse>> GetByStatusAsync(string status)
        {
            var shipments = await _shipmentRepo.GetByStatusAsync(status);
            var responses = new List<ShipmentResponse>();
            foreach (var shipment in shipments)
            {
                responses.Add(await MapToResponseWithTracking(shipment));
            }
            return responses;
        }

        public async Task<List<ShipmentResponse>> GetByShipperAsync(Guid shipperId)
        {
            var shipments = await _shipmentRepo.GetByShipperAsync(shipperId);
            var responses = new List<ShipmentResponse>();
            foreach (var shipment in shipments)
            {
                responses.Add(await MapToResponseWithTracking(shipment));
            }
            return responses;
        }

        public async Task<List<ShipmentResponse>> GetByShipperAndStatusAsync(Guid shipperId, string status)
        {
            var shipments = await _shipmentRepo.GetByShipperAndStatusAsync(shipperId, status);
            var responses = new List<ShipmentResponse>();
            foreach (var shipment in shipments)
            {
                responses.Add(await MapToResponseWithTracking(shipment));
            }
            return responses;
        }

        public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request)
        {
            // Check if order exists and is in appropriate status
            var order = await _orderRepo.GetByIdAsync(request.OrderId);
            if (order.IsNull())
                throw new ArgumentException("Order not found");

            if (order!.Status != "Paid" && order.Status != "Shipped")
                throw new ArgumentException("Order must be paid before creating shipment");

            // Check if shipment already exists for this order
            var existingShipment = await _shipmentRepo.GetByOrderIdAsync(request.OrderId);
            if (existingShipment != null)
                throw new ArgumentException("Shipment already exists for this order");

            var shipment = new Shipment
            {
                ShipmentId = Guid.NewGuid(),
                OrderId = request.OrderId,
                Carrier = request.Carrier,
                TrackingNumber = request.TrackingNumber ?? string.Empty,
                Status = request.Status,
                DeliveryAddress = order.ShippingAddress,
                DeliveryLat = order.ShippingLat,
                DeliveryLng = order.ShippingLng,
                CreatedAt = DateTime.UtcNow
            };

            await _shipmentRepo.AddAsync(shipment);
            await _shipmentRepo.SaveAsync();

            // Add initial tracking event for single location business
            await _trackingService.AddTrackingEventAsync(
                shipment.ShipmentId,
                request.Status,
                request.Status == "Preparing" ? "Đơn hàng đang được chuẩn bị tại cửa hàng" : "Đơn hàng đã sẵn sàng giao",
                "Cửa hàng của bạn"
            );

            // Update order status to Shipped if shipment is created
            if (request.Status == "Shipped")
            {
                order.Status = "Shipped";
                await _orderRepo.UpdateAsync(order);
                await _orderRepo.SaveAsync();
            }

            return await MapToResponseWithTracking(shipment);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateShipmentRequest request)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(id);
            if (shipment.IsNull()) return false;

            // Use null-forgiving operator after null check
            var nonNullShipment = shipment!;

            if (!string.IsNullOrEmpty(request.Carrier))
                nonNullShipment.Carrier = request.Carrier;

            if (request.TrackingNumber != null)
                nonNullShipment.TrackingNumber = request.TrackingNumber;

            if (request.ShipperId.HasValue)
                nonNullShipment.ShipperId = request.ShipperId;

            if (!string.IsNullOrEmpty(request.Status))
            {
                nonNullShipment.Status = request.Status;

                // Add tracking event for status change (simplified for single location)
                string description = request.Status switch
                {
                    "In transit" => "Shipper đã nhận đơn và đang trên đường giao đến bạn",
                    "Out for delivery" => "Shipper đang giao hàng tại địa chỉ của bạn",
                    "Delivered" => "Đơn hàng đã được giao thành công",
                    _ => $"Trạng thái cập nhật: {request.Status}"
                };

                await _trackingService.AddTrackingEventAsync(
                    id,
                    request.Status,
                    description
                );

                // Update order status based on shipment status
                var order = await _orderRepo.GetByIdAsync(nonNullShipment.OrderId);
                if (order != null)
                {
                    if (request.Status == "Shipped" && order.Status == "Paid")
                    {
                        order.Status = "Shipped";
                        await _orderRepo.UpdateAsync(order);
                    }
                    else if (request.Status == "Delivered" && order.Status == "Shipped")
                    {
                        order.Status = "Completed";
                        await _orderRepo.UpdateAsync(order);
                    }
                }
            }

            await _shipmentRepo.UpdateAsync(nonNullShipment);
            await _shipmentRepo.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _shipmentRepo.ExistsAsync(id)) return false;

            await _shipmentRepo.DeleteAsync(id);
            await _shipmentRepo.SaveAsync();

            return true;
        }

        public async Task<List<TrackingEventResponse>> GetTrackingEventsAsync(Guid shipmentId)
        {
            return await _trackingService.GetTrackingEventsAsync(shipmentId);
        }

        public async Task<bool> SyncTrackingFromCarrierAsync(Guid shipmentId)
        {
            return await _trackingService.SyncTrackingEventsAsync(shipmentId);
        }

        // GPS Tracking
        public async Task<bool> UpdateShipmentLocationAsync(Guid shipmentId, double lat, double lng)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(shipmentId);
            if (shipment.IsNull()) return false;

            // Use null-forgiving operator after null check
            var nonNullShipment = shipment!;

            nonNullShipment.CurrentLat = lat;
            nonNullShipment.CurrentLng = lng;
            nonNullShipment.LastLocationUpdate = DateTime.UtcNow;

            await _shipmentRepo.UpdateAsync(nonNullShipment);
            await _shipmentRepo.SaveAsync();

            return true;
        }

        public async Task<ShipmentLocationResponse?> GetShipmentLocationAsync(Guid shipmentId)
        {
            var shipment = await _shipmentRepo.GetByIdAsync(shipmentId);
            if (shipment.IsNull()) return null;

            // Use null-forgiving operator after null check
            var nonNullShipment = shipment!;

            return new ShipmentLocationResponse
            {
                ShipmentId = nonNullShipment.ShipmentId,
                CurrentLat = nonNullShipment.CurrentLat,
                CurrentLng = nonNullShipment.CurrentLng,
                LastLocationUpdate = nonNullShipment.LastLocationUpdate,
                Status = nonNullShipment.Status
            };
        }

        public async Task<List<ShipmentLocationResponse>> GetActiveShipmentsLocationsAsync()
        {
            // Get shipments that are in transit (not delivered or cancelled)
            var activeStatuses = new[] { "Preparing", "Picked up", "In transit", "Out for delivery" };
            var shipments = await _shipmentRepo.GetByStatusAsync(string.Join(",", activeStatuses));

            return shipments
                .Where(s => s.CurrentLat.HasValue && s.CurrentLng.HasValue)
                .Select(s => new ShipmentLocationResponse
                {
                    ShipmentId = s.ShipmentId,
                    CurrentLat = s.CurrentLat,
                    CurrentLng = s.CurrentLng,
                    LastLocationUpdate = s.LastLocationUpdate,
                    Status = s.Status
                })
                .ToList();
        }

        private async Task<ShipmentResponse> MapToResponse(Shipment shipment)
        {
            UserResponse? shipper = null;
            if (shipment.ShipperId.HasValue)
            {
                shipper = _authService.GetMe(shipment.ShipperId.Value);
            }

            // Get order to retrieve delivery address if shipment doesn't have it
            var order = await _orderRepo.GetByIdAsync(shipment.OrderId);
            var deliveryAddress = shipment.DeliveryAddress ?? order?.ShippingAddress;

            return new ShipmentResponse
            {
                ShipmentId = shipment.ShipmentId,
                OrderId = shipment.OrderId,
                ShipperId = shipment.ShipperId,
                Shipper = shipper,
                Carrier = shipment.Carrier,
                TrackingNumber = shipment.TrackingNumber,
                Status = shipment.Status,
                CreatedAt = shipment.CreatedAt,
                CurrentLat = shipment.CurrentLat,
                CurrentLng = shipment.CurrentLng,
                LastLocationUpdate = shipment.LastLocationUpdate,
                DeliveryAddress = deliveryAddress,
                DeliveryLat = shipment.DeliveryLat,
                DeliveryLng = shipment.DeliveryLng
            };
        }

        private async Task<ShipmentResponse> MapToResponseWithTracking(Shipment shipment)
        {
            var response = await MapToResponse(shipment);
            response.TrackingEvents = await _trackingService.GetTrackingEventsAsync(shipment.ShipmentId);
            return response;
        }
    }
}