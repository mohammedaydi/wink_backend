using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace wink.Models
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }


        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal? Total { get; set; }
    }
}
