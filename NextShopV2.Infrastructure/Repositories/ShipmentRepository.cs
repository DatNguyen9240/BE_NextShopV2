using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Payments;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly AppDbContext _context;

        public ShipmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Shipment>> GetAllAsync()
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .FirstOrDefaultAsync(s => s.ShipmentId == id);
        }

        public async Task<Shipment?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        public async Task<List<Shipment>> GetByStatusAsync(string status)
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .Where(s => s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Shipment>> GetByShipperAsync(Guid shipperId)
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .Where(s => s.ShipperId == shipperId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Shipment>> GetByShipperAndStatusAsync(Guid shipperId, string status)
        {
            return await _context.Shipments
                .Include(s => s.TrackingEvents)
                .Where(s => s.ShipperId == shipperId && s.Status == status)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Shipments.AnyAsync(s => s.ShipmentId == id);
        }

        public async Task AddAsync(Shipment shipment)
        {
            await _context.Shipments.AddAsync(shipment);
        }

        public async Task UpdateAsync(Shipment shipment)
        {
            _context.Shipments.Update(shipment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var shipment = await GetByIdAsync(id);
            if (shipment != null)
            {
                _context.Shipments.Remove(shipment);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}