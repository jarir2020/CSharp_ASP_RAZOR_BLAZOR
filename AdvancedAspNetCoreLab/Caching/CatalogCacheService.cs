using System.Text.Json;
using AdvancedAspNetCoreLab.Configuration;
using AdvancedAspNetCoreLab.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AdvancedAspNetCoreLab.Caching;

public sealed class CatalogCacheService
{
    private const string CacheKey = "course-catalog";
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly IOptions<CoursePlatformOptions> _options;
    private readonly ILogger<CatalogCacheService> _logger;

    public CatalogCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IOptions<CoursePlatformOptions> options,
        ILogger<CatalogCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _options = options;
        _logger = logger;
    }

    public async Task<CatalogSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CacheKey"] = CacheKey
        });

        if (_memoryCache.TryGetValue(CacheKey, out CatalogSnapshot? memorySnapshot)
            && memorySnapshot is not null)
        {
            _logger.LogInformation("Catalog served from memory cache");
            return memorySnapshot with { Source = "memory" };
        }

        string? distributedJson = await _distributedCache.GetStringAsync(
            CacheKey,
            cancellationToken);

        if (distributedJson is not null)
        {
            CatalogSnapshot? distributedSnapshot = JsonSerializer.Deserialize<CatalogSnapshot>(distributedJson);

            if (distributedSnapshot is not null)
            {
                _memoryCache.Set(CacheKey, distributedSnapshot, MemoryLifetime());
                _logger.LogInformation("Catalog promoted from distributed cache to memory cache");
                return distributedSnapshot with { Source = "distributed" };
            }
        }

        CatalogSnapshot originSnapshot = new(
            DateTimeOffset.UtcNow,
            new[]
            {
                new CatalogItem(1, "ASP.NET Core Fundamentals", "Beginner"),
                new CatalogItem(2, "Advanced Dependency Injection", "Advanced"),
                new CatalogItem(3, "Reliable Background Work", "Advanced")
            },
            "origin");

        _memoryCache.Set(CacheKey, originSnapshot, MemoryLifetime());
        await _distributedCache.SetStringAsync(
            CacheKey,
            JsonSerializer.Serialize(originSnapshot),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheLifetime()
            },
            cancellationToken);

        _logger.LogInformation("Catalog generated and stored in both cache layers");
        return originSnapshot;
    }

    private MemoryCacheEntryOptions MemoryLifetime()
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheLifetime()
        };
    }

    private TimeSpan CacheLifetime()
    {
        return TimeSpan.FromSeconds(_options.Value.MemoryCacheSeconds);
    }
}
