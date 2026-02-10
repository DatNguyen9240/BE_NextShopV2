using Microsoft.EntityFrameworkCore;
using NextShopV2.Application.Interfaces.Repositories;
using NextShopV2.Domain.Entities.Orders;
using NextShopV2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .Include(o => o.Payments)
                .Include(o => o.Shipment)
                .Include(o => o.OrderCoupons)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<int> CountOrderCouponsByCouponIdAsync(Guid couponId, params string[] statuses)
        {
            var query = _context.OrderCoupons.AsQueryable().Where(oc => oc.CouponId == couponId);
            if (statuses != null && statuses.Length > 0)
                query = query.Where(oc => statuses.Contains(oc.Status));
            return await query.CountAsync();
        }

        public async Task<Dictionary<Guid, int>> CountOrderCouponsByCouponIdsAsync(List<Guid> couponIds, params string[] statuses)
        {
            if (couponIds == null || !couponIds.Any())
                return new Dictionary<Guid, int>();

            var query = _context.OrderCoupons.AsQueryable().Where(oc => couponIds.Contains(oc.CouponId));
            if (statuses != null && statuses.Length > 0)
                query = query.Where(oc => statuses.Contains(oc.Status));

            var grouped = await query
                .GroupBy(oc => oc.CouponId)
                .Select(g => new { CouponId = g.Key, Count = g.Count() })
                .ToListAsync();

            return grouped.ToDictionary(x => x.CouponId, x => x.Count);
        }

        public async Task<List<Order>> GetByIdsAsync(List<Guid> orderIds)
        {
            if (orderIds == null || !orderIds.Any())
                return new List<Order>();

            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .Include(o => o.Payments)
                .Include(o => o.Shipment)
                .Include(o => o.OrderCoupons)
                .Where(o => orderIds.Contains(o.OrderId))
                .ToListAsync();
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<(List<Order> Items, int TotalCount)> GetByUserIdPagedAsync(Guid userId, int page, int pageSize, string? status = null)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .Where(o => o.UserId == userId && (string.IsNullOrEmpty(status) || o.Status == status));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<List<Order>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v!.Product)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == id);
        }

        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}