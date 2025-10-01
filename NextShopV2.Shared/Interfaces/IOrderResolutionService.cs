using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Interfaces
{
    /// <summary>
    /// Service interface for resolving order conflicts across different entities
    /// </summary>
    public interface IOrderResolutionService
    {
        /// <summary>
        /// Resolve order conflict by incrementing until unique
        /// </summary>
        /// <param name="existingOrders">List of existing orders to check against</param>
        /// <param name="requestedOrder">The requested order value</param>
        /// <param name="excludeOrder">Order to exclude from conflict check (for updates)</param>
        /// <returns>Resolved unique order</returns>
        int ResolveOrder(IEnumerable<int> existingOrders, int requestedOrder, int? excludeOrder = null);

        /// <summary>
        /// Async version for resolving order conflicts
        /// </summary>
        Task<int> ResolveOrderAsync(IEnumerable<int> existingOrders, int requestedOrder, int? excludeOrder = null);
    }
}