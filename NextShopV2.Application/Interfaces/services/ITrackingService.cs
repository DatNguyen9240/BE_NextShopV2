using NextShopV2.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface ITrackingService
    {
        Task<bool> SyncTrackingEventsAsync(Guid shipmentId);
        Task<List<TrackingEventResponse>> GetTrackingEventsAsync(Guid shipmentId);
        Task<bool> AddTrackingEventAsync(Guid shipmentId, string status, string description, string? location = null);
    }
}