using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Request.UpdateDto;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;
using Microsoft.AspNetCore.SignalR;
using NextShopV2.Api.Hubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipperController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly IHubContext<ShipmentTrackingHub> _hubContext;

        public ShipperController(IShipmentService shipmentService, IHubContext<ShipmentTrackingHub> hubContext)
        {
            _shipmentService = shipmentService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Shipper updates their current location for a shipment
        /// </summary>
        [HttpPut("shipments/{shipmentId}/location")]
        [ShipperOnly]
        public async Task<IActionResult> UpdateShipmentLocation(Guid shipmentId, [FromBody] UpdateShipmentLocationRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var success = await _shipmentService.UpdateShipmentLocationAsync(shipmentId, request.Lat, request.Lng);
            if (!success)
                return ResponseHelper.NotFound("Shipment not found");

            // Broadcast location update to all clients tracking this shipment
            await _hubContext.Clients.Group($"shipment-{shipmentId}").SendAsync("LocationUpdated", new
            {
                ShipmentId = shipmentId,
                Lat = request.Lat,
                Lng = request.Lng,
                Timestamp = DateTime.UtcNow
            });

            return ResponseHelper.Success("Location updated successfully");
        }

        /// <summary>
        /// Get current location of a shipment (for customers to track)
        /// </summary>
        [HttpGet("shipments/{shipmentId}/location")]
        [Authenticated]
        public async Task<IActionResult> GetShipmentLocation(Guid shipmentId)
        {
            var location = await _shipmentService.GetShipmentLocationAsync(shipmentId);
            if (location == null)
                return ResponseHelper.NotFound("Shipment not found or no location data");

            return ResponseHelper.Success(location);
        }

        /// <summary>
        /// Get locations of all active shipments (for map view)
        /// </summary>
        [HttpGet("shipments/active-locations")]
        [Authenticated]
        public async Task<IActionResult> GetActiveShipmentsLocations()
        {
            var locations = await _shipmentService.GetActiveShipmentsLocationsAsync();
            return ResponseHelper.Success(locations);
        }

        /// <summary>
        /// Shipper marks shipment as delivered
        /// </summary>
        [HttpPut("shipments/{shipmentId}/deliver")]
        [ShipperOnly]
        public async Task<IActionResult> MarkAsDelivered(Guid shipmentId)
        {
            var updateRequest = new UpdateShipmentRequest
            {
                Status = "Delivered"
            };

            var success = await _shipmentService.UpdateAsync(shipmentId, updateRequest);
            if (!success)
                return ResponseHelper.NotFound("Shipment not found");

            return ResponseHelper.Success("Shipment marked as delivered");
        }

        /// <summary>
        /// Shipper starts delivery for a shipment
        /// </summary>
        [HttpPut("shipments/{shipmentId}/start-delivery")]
        [ShipperOnly]
        public async Task<IActionResult> StartDelivery(Guid shipmentId)
        {
            var updateRequest = new UpdateShipmentRequest
            {
                Status = "In transit"
            };

            var success = await _shipmentService.UpdateAsync(shipmentId, updateRequest);
            if (!success)
                return ResponseHelper.NotFound("Shipment not found");

            return ResponseHelper.Success("Delivery started");
        }

        /// <summary>
        /// Get shipments assigned to current shipper (simplified for single location)
        /// </summary>
        [HttpGet("my-shipments")]
        [ShipperOnly]
        public async Task<IActionResult> GetMyShipments([FromQuery] string? status = null)
        {
            // Get current shipper ID from claims
            var shipperIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(shipperIdClaim) || !Guid.TryParse(shipperIdClaim, out var shipperId))
                return ResponseHelper.Unauthorized("Invalid shipper identity");

            // Get shipments assigned to this shipper
            var shipments = status != null
                ? await _shipmentService.GetByShipperAndStatusAsync(shipperId, status)
                : await _shipmentService.GetByShipperAsync(shipperId);

            return ResponseHelper.Success(shipments);
        }
    }
}