using E_commerce_Web_App_Backend_Services.models;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class ProductDTO
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("product_id")]
        public string ProductId { get; set; }


        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("vendor_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string VendorId { get; set; }

        public string VendorName { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }

        [BsonElement("stockQuantity")]
        public int stockQuantity { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
