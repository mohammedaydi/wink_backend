using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace wink.Models
{
    public class CartItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ItemId { get; set; }

        public int Quantity { get; set; }

    }
}
