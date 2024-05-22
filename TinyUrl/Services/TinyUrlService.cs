using System.Collections.Concurrent;
using TinyUrl.Models;
using TinyUrl.Utils;

namespace TinyUrl.Services
{
    public class TinyUrlService
    {
        private readonly ILogger<TinyUrlService> _logger;

        private readonly DbService _db;

        /// <summary>
        /// Cache used to check if short/long urls exists (both short and long url can be stored as keys).
        /// </summary>
        private readonly LFUCache<UrlMapping> _cache;

        /// <summary>
        /// Conqurrent dictionary for request collapsing.
        /// </summary>
        private readonly ConcurrentDictionary<string, Task<string>> _ongoingRequests;

        public TinyUrlService(ILogger<TinyUrlService> logger, DbService dbService, int cacheSize)
        {
            _logger = logger;
            _db = dbService;
            _cache = new LFUCache<UrlMapping>(cacheSize);
            _ongoingRequests = new ConcurrentDictionary<string, Task<string>>();
        }

        public async Task<string?> CreateShortUrlAsync(string longUrl)
        {
            _logger.LogInformation("Received request to shorten URL: {longUrl}", longUrl);

            // check if long url in cache
            var cachedItem = _cache.Get(longUrl);
            if (cachedItem != null)
            {
                _logger.LogInformation("Cache hit for URL: {LongUrl}", longUrl);
                return cachedItem.ShortUrl;
            }

            return await _ongoingRequests.GetOrAdd(longUrl, async key =>
            {
                var existingMapping = await _db.GetByLongUrlAsync(key);

                if (existingMapping != null)
                {
                    _logger.LogInformation("Found existing mapping for URL: {LongUrl} -> {ShortUrl}", longUrl, existingMapping.ShortUrl);

                    _cache.Put(key, existingMapping);
                    _ongoingRequests.TryRemove(key, out _);
                    return existingMapping.ShortUrl;
                }

                string shortUrl = string.Empty;
                bool hasCollision = false;

                // continue generate short url until we have no coliision
                do
                {
                    shortUrl = GenerateShortUrl(longUrl);
                    longUrl += "1"; // modify to receive different hash on next try
                    hasCollision = await _db.GetAsync(shortUrl) != null;

                    if (hasCollision)
                    {
                        _logger.LogWarning("Collision detected for short URL: {ShortUrl}. Regenerating.", shortUrl);
                    }
                } while (hasCollision);

                var newMapping = new UrlMapping
                {
                    ShortUrl = shortUrl,
                    LongUrl = key
                };

                await _db.CreateAsync(newMapping);
                _cache.Put(key, newMapping);
                _ongoingRequests.TryRemove(key, out _);

                _logger.LogInformation("Stored new mapping: {LongUrl} -> {ShortUrl}", longUrl, shortUrl);

                return shortUrl;
            });
        }

        public async Task<string> GetLongUrlAsync(string shortUrl)
        {
            _logger.LogInformation("Received request to expand short URL: {ShortUrl}", shortUrl);

            // check if short url in cache
            var cachedItem = _cache.Get(shortUrl);
            if (cachedItem != null)
            {
                _logger.LogInformation("Cache hit for short URL: {ShortUrl}", shortUrl);
                return cachedItem.LongUrl;
            }

            return await _ongoingRequests.GetOrAdd(shortUrl, async key =>
            {
                var urlMapping = await _db.GetAsync(key);
                if (urlMapping != null)
                {
                    _logger.LogInformation("Found mapping for short URL: {ShortUrl} -> {LongUrl}", shortUrl, urlMapping.LongUrl);

                    _cache.Put(key, urlMapping);
                    _ongoingRequests.TryRemove(key, out _);
                    return urlMapping.LongUrl;
                }

                _logger.LogWarning("No mapping found for short URL: {ShortUrl}", shortUrl);

                _ongoingRequests.TryRemove(key, out _);
                return string.Empty;
            });
        }

        private static string GenerateShortUrl(string longUrl)
        {
            var hashBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(longUrl));
            return Convert.ToBase64String(hashBytes)[..6].Replace('+', '-').Replace('/', '_');
        }
    }
}
