using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("is_read")]
        public bool IsRead { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
