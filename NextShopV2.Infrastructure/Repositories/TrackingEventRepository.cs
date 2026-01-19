using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Payments;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NextShopV2.Infrastructure.Repositories
{
    public class TrackingEventRepository : ITrackingEventRepository
    {
        private readonly AppDbContext _context;

        public TrackingEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrackingEvent>> GetByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.TrackingEvents
                .Where(te => te.ShipmentId == shipmentId)
                .OrderBy(te => te.EventTime)
                .ToListAsync();
        }

        public async Task AddAsync(TrackingEvent trackingEvent)
        {
            await _context.TrackingEvents.AddAsync(trackingEvent);
        }

        public async Task AddRangeAsync(IEnumerable<TrackingEvent> trackingEvents)
        {
            await _context.TrackingEvents.AddRangeAsync(trackingEvents);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}