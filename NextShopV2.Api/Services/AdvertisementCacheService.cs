using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using NextShopV2.Infrastructure.Persistence;
using NextShopV2.Domain.Entities.Marketing;

namespace NextShopV2.Api.Services
{
    public class AdvertisementCacheService
    {
        private readonly AppDbContext _context;
        private readonly IConnectionMultiplexer _redis;
        private readonly TimeSpan _expiry = TimeSpan.FromMinutes(5);

        public AdvertisementCacheService(AppDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        public async Task UpdateBannerCacheAsync()
        {
            var cacheKey = "banners:all";
            var db = _redis.GetDatabase();
            var banners = await _context.Advertisements.OrderBy(a => a.SortOrder).ToListAsync();
            await db.StringSetAsync(cacheKey, JsonConvert.SerializeObject(banners), _expiry);
        }
    }
}