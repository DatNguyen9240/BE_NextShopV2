using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Request.UpdateDto;
using NextShopV2.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IShipmentService
    {
        Task<List<ShipmentResponse>> GetAllAsync();
        Task<ShipmentResponse?> GetByIdAsync(Guid id);
        Task<ShipmentResponse?> GetByOrderIdAsync(Guid orderId);
        Task<List<ShipmentResponse>> GetByStatusAsync(string status);
        Task<List<ShipmentResponse>> GetByShipperAsync(Guid shipperId);
        Task<List<ShipmentResponse>> GetByShipperAndStatusAsync(Guid shipperId, string status);
        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateShipmentRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<List<TrackingEventResponse>> GetTrackingEventsAsync(Guid shipmentId);
        Task<bool> SyncTrackingFromCarrierAsync(Guid shipmentId);

        // GPS Tracking
        Task<bool> UpdateShipmentLocationAsync(Guid shipmentId, double lat, double lng);
        Task<ShipmentLocationResponse?> GetShipmentLocationAsync(Guid shipmentId);
        Task<List<ShipmentLocationResponse>> GetActiveShipmentsLocationsAsync();
    }
}