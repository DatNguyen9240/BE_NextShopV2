using NextShopV2.Shared.Interfaces;
using NextShopV2.Application.DTOs;
using NextShopV2.Application.DTOs.Response;
using NextShopV2.Application.DTOs.Request.CreateDto;
using NextShopV2.Shared.Helpers;
using NextShopV2.Application.Interfaces.Services;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NextShopV2.Infrastructure.Services
{
    public class RedisCartService : IRedisCartService
    {
        private readonly IDatabase _redisDb;
        private readonly IProductVariantCartService _variantService;
        private const int CART_EXPIRE_DAYS = 30;

        public RedisCartService(IDatabase redisDb, IProductVariantCartService variantService)
        {
            _redisDb = redisDb;
            _variantService = variantService;
        }

        public async Task<CartDto> GetCartAsync(Guid userId)
        {
            var cartKey = GetCartKey(userId);
            var cartData = await _redisDb.HashGetAllAsync(cartKey);

            if (!cartData.Any())
            {
                return new CartDto
                {
                    CartId = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItemDto>(),
                    TotalAmount = 0,
                    TotalItems = 0
                };
            }

            var cartItems = new List<CartItemDto>();
            var cartId = Guid.Parse(cartData.FirstOrDefault(x => x.Name == "cartId").Value.ToString());
            var createdAt = DateTime.Parse(cartData.FirstOrDefault(x => x.Name == "createdAt").Value!);

            foreach (var item in cartData.Where(x => x.Name.ToString().StartsWith("item:")))
            {
                var redisItem = JsonSerializer.Deserialize<RedisCartItem>(item.Value.ToString());
                if (redisItem != null)
                {
                    var variantInfo = await _variantService.GetVariantInfoAsync(redisItem.VariantId);
                    if (variantInfo != null)
                    {
                        cartItems.Add(new CartItemDto
                        {
                            CartItemId = redisItem.CartItemId,
                            VariantId = redisItem.VariantId,
                            Quantity = redisItem.Quantity,
                            UnitPrice = variantInfo.Price,
                            TotalPrice = variantInfo.Price * redisItem.Quantity,
                            VariantInfo = variantInfo
                        });
                    }
                }
            }

            return new CartDto
            {
                CartId = cartId,
                UserId = userId,
                CreatedAt = createdAt,
                Items = cartItems,
                TotalAmount = cartItems.Sum(item => item.TotalPrice),
                TotalItems = cartItems.Sum(item => item.Quantity)
            };
        }

        public async Task<CartDto> AddToCartAsync(Guid userId, AddCartItemDto request)
        {
            // Validate variant
            var variantInfo = await _variantService.GetVariantInfoAsync(request.VariantId);
            if (variantInfo == null)
                throw new ArgumentException("Product variant not found");

            if (!variantInfo.IsActive)
                throw new ArgumentException($"Variant {request.VariantId} is inactive");

            if (!variantInfo.ProductIsActive)
                throw new ArgumentException($"Product {variantInfo.ProductId} is inactive");

            if (variantInfo.StockQuantity < request.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {variantInfo.StockQuantity}");

            var cartKey = GetCartKey(userId);
            var itemKey = $"item:{request.VariantId}";

            // Ensure cart exists
            await EnsureCartExistsAsync(cartKey, userId);

            // Check if item already exists
            var existingItemJson = await _redisDb.HashGetAsync(cartKey, itemKey);
            if (existingItemJson.HasValue)
            {
                var existingItem = JsonSerializer.Deserialize<RedisCartItem>(existingItemJson.ToString());
                if (existingItem != null)
                {
                    var newQuantity = existingItem.Quantity + request.Quantity;
                    if (variantInfo.StockQuantity < newQuantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {variantInfo.StockQuantity}, Requested: {newQuantity}");

                    existingItem.Quantity = newQuantity;
                    await _redisDb.HashSetAsync(cartKey, itemKey, JsonSerializer.Serialize(existingItem));
                }
            }
            else
            {
                // Add new item
                var newItem = new RedisCartItem
                {
                    CartItemId = Guid.NewGuid(),
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow
                };
                await _redisDb.HashSetAsync(cartKey, itemKey, JsonSerializer.Serialize(newItem));
            }

            await _redisDb.KeyExpireAsync(cartKey, TimeSpan.FromDays(CART_EXPIRE_DAYS));
            return await GetCartAsync(userId);
        }

        public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, int quantity)
        {
            var cartKey = GetCartKey(userId);
            var cartData = await _redisDb.HashGetAllAsync(cartKey);

            if (!cartData.Any())
                throw new ArgumentException("Cart not found");

            foreach (var item in cartData.Where(x => x.Name.ToString().StartsWith("item:")))
            {
                var redisItem = JsonSerializer.Deserialize<RedisCartItem>(item.Value.ToString());
                if (redisItem?.CartItemId == cartItemId)
                {
                    // Validate stock
                    var variantInfo = await _variantService.GetVariantInfoAsync(redisItem.VariantId);
                    if (variantInfo == null)
                        throw new ArgumentException("Product variant not found");

                    if (!variantInfo.IsActive)
                        throw new ArgumentException($"Variant {redisItem.VariantId} is inactive");

                    if (!variantInfo.ProductIsActive)
                        throw new ArgumentException($"Product {variantInfo.ProductId} is inactive");

                    if (variantInfo.StockQuantity < quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {variantInfo.StockQuantity}");

                    redisItem.Quantity = quantity;
                    await _redisDb.HashSetAsync(cartKey, item.Name!, JsonSerializer.Serialize(redisItem));
                    await _redisDb.KeyExpireAsync(cartKey, TimeSpan.FromDays(CART_EXPIRE_DAYS));
                    break;
                }
            }

            return await GetCartAsync(userId);
        }

        public async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
        {
            var cartKey = GetCartKey(userId);
            var cartData = await _redisDb.HashGetAllAsync(cartKey);

            if (!cartData.Any())
                return false;

            foreach (var item in cartData.Where(x => x.Name.ToString().StartsWith("item:")))
            {
                var redisItem = JsonSerializer.Deserialize<RedisCartItem>(item.Value.ToString());
                if (redisItem?.CartItemId == cartItemId)
                {
                    await _redisDb.HashDeleteAsync(cartKey, item.Name!);
                    await _redisDb.KeyExpireAsync(cartKey, TimeSpan.FromDays(CART_EXPIRE_DAYS));
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var cartKey = GetCartKey(userId);
            return await _redisDb.KeyDeleteAsync(cartKey);
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            var cartKey = GetCartKey(userId);
            var cartData = await _redisDb.HashGetAllAsync(cartKey);

            var totalCount = 0;
            foreach (var item in cartData.Where(x => x.Name.ToString().StartsWith("item:")))
            {
                var redisItem = JsonSerializer.Deserialize<RedisCartItem>(item.Value.ToString());
                if (redisItem != null)
                {
                    totalCount += redisItem.Quantity;
                }
            }

            return totalCount;
        }

        private async Task EnsureCartExistsAsync(string cartKey, Guid userId)
        {
            var cartExists = await _redisDb.HashExistsAsync(cartKey, "cartId");
            if (!cartExists)
            {
                var cartId = Guid.NewGuid();
                await _redisDb.HashSetAsync(cartKey, new HashEntry[]
                {
                    new("cartId", cartId.ToString()),
                    new("userId", userId.ToString()),
                    new("createdAt", DateTime.UtcNow.ToString("O"))
                });
                await _redisDb.KeyExpireAsync(cartKey, TimeSpan.FromDays(CART_EXPIRE_DAYS));
            }
        }

        private static string GetCartKey(Guid userId) => $"cart:{userId}";
    }

    internal class RedisCartItem
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}