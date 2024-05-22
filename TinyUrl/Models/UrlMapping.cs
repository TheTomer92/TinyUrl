using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TinyUrl.Models
{
    public class UrlMapping
    {
        [BsonId]
        [BsonElement("shortUrl")]
        public required string ShortUrl { get; set; }

        [BsonElement("longUrl")]
        public required string LongUrl { get; set; }
    }
}

