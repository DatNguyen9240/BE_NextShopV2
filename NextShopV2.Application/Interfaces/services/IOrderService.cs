using NextShopV2.Application.DTOs.Request;
using NextShopV2.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<List<OrderResponse>> GetAllAsync();
        Task<OrderResponse?> GetByIdAsync(Guid id);
        Task<List<OrderResponse>> GetByUserIdAsync(Guid userId);
        Task<List<OrderResponse>> GetByStatusAsync(string status);
        Task<OrderResponse> CreateAsync(Guid userId, CreateOrderRequest request);
        Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request);
        Task<bool> CancelOrderAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<decimal> CalculateOrderTotalAsync(CreateOrderRequest request);
    }
}