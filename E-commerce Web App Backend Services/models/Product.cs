using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("product_id")]
        public string ProductId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("vendor_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VendorId { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // "active", "deactivated"

        [BsonElement("ratings")]
        public List<ProductRating> Ratings { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductRating
    {
        [BsonElement("customer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }

        [BsonElement("comment")]
        public string Comment { get; set; }
    }
}
