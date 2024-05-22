using TinyUrl.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TinyUrl.Services;

public class DbService
{
    private readonly IMongoCollection<UrlMapping> _urlMappings;

    public DbService(IOptions<DbSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);

        _urlMappings = mongoDatabase.GetCollection<UrlMapping>(dbSettings.Value.CollectionName);
    }

    public async Task<List<UrlMapping>> GetAsync() =>
        await _urlMappings.Find(_ => true).ToListAsync();

    public async Task<UrlMapping?> GetAsync(string shortUrl) =>
        await _urlMappings.Find(x => x.ShortUrl == shortUrl).FirstOrDefaultAsync();

    public async Task<UrlMapping?> GetByLongUrlAsync(string longUrl) =>
        await _urlMappings.Find(x => x.LongUrl == longUrl).FirstOrDefaultAsync();

    public async Task CreateAsync(UrlMapping mapping) =>
        await _urlMappings.InsertOneAsync(mapping);

    public async Task UpdateAsync(string shortUrl, UrlMapping newMapping) =>
        await _urlMappings.ReplaceOneAsync(x => x.ShortUrl == shortUrl, newMapping);

    public async Task RemoveAsync(string shortUrl) =>
        await _urlMappings.DeleteOneAsync(x => x.ShortUrl == shortUrl);
}