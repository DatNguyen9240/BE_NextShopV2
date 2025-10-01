using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextShopV2.Shared.Extensions
{
    /// <summary>
    /// Extension methods for collections and objects to handle null checks and operations
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Check if object is null
        /// </summary>
        public static bool IsNull<T>(this T? obj) where T : class
        {
            return obj is null;
        }

        /// <summary>
        /// Check if object is not null
        /// </summary>
        public static bool IsNotNull<T>(this T? obj) where T : class
        {
            return obj is not null;
        }

        /// <summary>
        /// Check if collection is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        /// Safe async foreach that handles null collections
        /// </summary>
        public static async Task SafeForEachAsync<T>(this IEnumerable<T>? collection, Func<T, Task> asyncAction)
        {
            if (collection != null && asyncAction != null)
            {
                foreach (var item in collection)
                {
                    await asyncAction(item);
                }
            }
        }

        /// <summary>
        /// Get random item from collection
        /// </summary>
        public static T? GetRandomItem<T>(this IEnumerable<T> collection)
        {
            var list = collection.ToList();
            if (!list.Any()) return default(T);
            
            var random = new Random();
            return list[random.Next(list.Count)];
        }
    }
}