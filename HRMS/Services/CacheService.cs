using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using HRMS.IServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HRMS.Services;

public class CacheService(IDistributedCache cache, ILogger<CacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var bytes = await cache.GetAsync(key, ct);
            if (bytes == null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DistributedCache error retrieving key '{Key}'. Skipping cache.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };
            await cache.SetAsync(key, bytes, options, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DistributedCache error setting key '{Key}'. Skipping cache.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DistributedCache error removing key '{Key}'.", key);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache HIT for key '{Key}'", key);
            return cached;
        }

        logger.LogDebug("Cache MISS for key '{Key}'. Fetching from data source...", key);
        var item = await factory();
        if (item is not null)
        {
            await SetAsync(key, item, expiration, ct);
        }

        return item;
    }
}
