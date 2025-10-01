using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NextShopV2.Shared.Interfaces;

namespace NextShopV2.Shared.Services
{
    /// <summary>
    /// Service to handle order resolution for various entities (DisplayOrder, SortOrder, etc.)
    /// This is a shared utility that can be used across different domains
    /// </summary>
    public class OrderResolutionService : IOrderResolutionService
    {
        /// <summary>
        /// Resolve order conflict by incrementing until unique
        /// </summary>
        /// <param name="existingOrders">List of existing orders to check against</param>
        /// <param name="requestedOrder">The requested order value</param>
        /// <param name="excludeOrder">Order to exclude from conflict check (for updates)</param>
        /// <returns>Resolved unique order</returns>
        public int ResolveOrder(IEnumerable<int> existingOrders, int requestedOrder, int? excludeOrder = null)
        {
            var ordersToCheck = excludeOrder.HasValue 
                ? existingOrders.Where(o => o != excludeOrder.Value).ToHashSet()
                : existingOrders.ToHashSet();
            
            var resolvedOrder = requestedOrder;
            
            // Keep incrementing until we find an available order
            while (ordersToCheck.Contains(resolvedOrder))
            {
                resolvedOrder++;
            }
            
            return resolvedOrder;
        }

        /// <summary>
        /// Async version for resolving order conflicts
        /// </summary>
        public Task<int> ResolveOrderAsync(IEnumerable<int> existingOrders, int requestedOrder, int? excludeOrder = null)
        {
            return Task.FromResult(ResolveOrder(existingOrders, requestedOrder, excludeOrder));
        }
    }
}