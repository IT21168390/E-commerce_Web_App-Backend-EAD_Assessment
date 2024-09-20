using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace E_commerce_Web_App_Backend_Services.models
{
    public class VendorRating {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("customer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }

        [BsonElement("vendor_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VendorId { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
