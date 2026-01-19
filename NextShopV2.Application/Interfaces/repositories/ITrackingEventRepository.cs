using NextShopV2.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface ITrackingEventRepository
    {
        Task<List<TrackingEvent>> GetByShipmentIdAsync(Guid shipmentId);
        Task AddAsync(TrackingEvent trackingEvent);
        Task AddRangeAsync(IEnumerable<TrackingEvent> trackingEvents);
        Task SaveAsync();
    }
}