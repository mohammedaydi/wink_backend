using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace wink.Models
{
    public class Item
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Title")]
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; } = null!;

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            var other = obj as Item;
            if (other == null) return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            if( Id == null ) return 0;
            int hash = 13;

            hash = hash * 397 + Id.GetHashCode();
            hash = hash * 397 + Name.GetHashCode();
            return hash;
        }
    }
}
