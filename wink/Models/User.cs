using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace wink.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public decimal Balance { get; set; }
}