using System.Collections.Generic;
using System.Linq;

namespace NextShopV2.Shared.Extensions
{
    /// <summary>
    /// Extension methods for order resolution
    /// </summary>
    public static class OrderResolutionExtensions
    {
        /// <summary>
        /// Resolve order conflict by incrementing until unique
        /// </summary>
        /// <param name="existingOrders">Existing orders to check against</param>
        /// <param name="requestedOrder">The requested order value</param>
        /// <param name="excludeOrder">Order to exclude from conflict check (for updates)</param>
        /// <returns>Resolved unique order</returns>
        public static int ResolveOrderConflict(this IEnumerable<int> existingOrders, int requestedOrder, int? excludeOrder = null)
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
    }
}