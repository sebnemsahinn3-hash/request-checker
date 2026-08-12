using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PTN.WebAPI.Extensions
{
    public static class DistributedCacheExtensions
    {
        // Cache'te varsa getirir, yoksa veritabanından çekip Cache'e ekler (GetOrSet)
        public static async Task<T?> GetOrSetAsync<T>(
            this IDistributedCache cache,
            string key,
            Func<Task<T>> getItemCallback,
            TimeSpan? absoluteExpireTime = null,
            CancellationToken cancellationToken = default)
        {
            var cachedData = await cache.GetStringAsync(key, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            var item = await getItemCallback();

            if (item != null)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(10)
                };

                var serializedData = JsonSerializer.Serialize(item);
                await cache.SetStringAsync(key, serializedData, options, cancellationToken);
            }

            return item;
        }
    }
}