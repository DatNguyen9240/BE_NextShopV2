using Microsoft.AspNetCore.Mvc;
using NextShopV2.Application.Interfaces.Services;
using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Request.UpdateDto;
using NextShopV2.Shared.Helpers;
using NextShopV2.Api.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Api.Controllers
{
    [ApiController]
    [Route("api/shipments")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet]
        [AdminOrShipper]
        public async Task<IActionResult> GetAll()
        {
            var shipments = await _shipmentService.GetAllAsync();
            return ResponseHelper.Success(shipments);
        }

        [HttpGet("{id}")]
        [AdminOrShipper]
        public async Task<IActionResult> GetById(Guid id)
        {
            var shipment = await _shipmentService.GetByIdAsync(id);
            if (shipment is null)
                return ResponseHelper.NotFound("Shipment not found");

            return ResponseHelper.Success(shipment);
        }

        [HttpGet("order/{orderId}")]
        [Authenticated]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var shipment = await _shipmentService.GetByOrderIdAsync(orderId);
            if (shipment is null)
                return ResponseHelper.NotFound("Shipment not found for this order");

            return ResponseHelper.Success(shipment);
        }

        [HttpGet("status/{status}")]
        [AdminOrShipper]
        public async Task<IActionResult> GetByStatus(string status)
        {
            var shipments = await _shipmentService.GetByStatusAsync(status);
            return ResponseHelper.Success(shipments);
        }

        [HttpPost]
        [AdminOrShipper]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            try
            {
                var result = await _shipmentService.CreateAsync(request);
                return ResponseHelper.Created(result, "Shipment created successfully");
            }
            catch (ArgumentException ex)
            {
                return ResponseHelper.BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [AdminOrShipper]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipmentRequest request)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ValidationError(ModelState);

            var success = await _shipmentService.UpdateAsync(id, request);
            return success ?
                ResponseHelper.Success("Shipment updated successfully") :
                ResponseHelper.NotFound("Shipment not found");
        }

        [HttpDelete("{id}")]
        [AdminOnly]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _shipmentService.DeleteAsync(id);
            return success ?
                ResponseHelper.Success("Shipment deleted successfully") :
                ResponseHelper.NotFound("Shipment not found");
        }

        // GET: api/Shipments/{id}/tracking
        [HttpGet("{id}/tracking")]
        [Authenticated]
        public async Task<IActionResult> GetTrackingEvents(Guid id)
        {
            var events = await _shipmentService.GetTrackingEventsAsync(id);
            return ResponseHelper.Success(events);
        }

        // POST: api/Shipments/{id}/sync-tracking
        [HttpPost("{id}/sync-tracking")]
        [AdminOrShipper]
        public async Task<IActionResult> SyncTrackingFromCarrier(Guid id)
        {
            var success = await _shipmentService.SyncTrackingFromCarrierAsync(id);
            return success ?
                ResponseHelper.Success("Tracking synced successfully") :
                ResponseHelper.BadRequest("Failed to sync tracking");
        }
    }
}