using NextShopV2.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Repositories
{
    public interface IShipmentRepository
    {
        Task<List<Shipment>> GetAllAsync();
        Task<Shipment?> GetByIdAsync(Guid id);
        Task<Shipment?> GetByOrderIdAsync(Guid orderId);
        Task<List<Shipment>> GetByStatusAsync(string status);
        Task<List<Shipment>> GetByShipperAsync(Guid shipperId);
        Task<List<Shipment>> GetByShipperAndStatusAsync(Guid shipperId, string status);
        Task<bool> ExistsAsync(Guid id);
        Task AddAsync(Shipment shipment);
        Task UpdateAsync(Shipment shipment);
        Task DeleteAsync(Guid id);
        Task SaveAsync();
    }
}