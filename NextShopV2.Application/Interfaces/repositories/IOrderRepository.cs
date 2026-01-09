using NextShopV2.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task<List<Order>> GetByUserIdAsync(Guid userId);
        Task<(List<Order> Items, int TotalCount)> GetByUserIdPagedAsync(Guid userId, int page, int pageSize, string? status = null);
        Task<List<Order>> GetByStatusAsync(string status);
        Task<bool> ExistsAsync(Guid id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
    }
}