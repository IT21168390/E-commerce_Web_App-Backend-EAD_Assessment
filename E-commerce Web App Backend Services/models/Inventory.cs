using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.models
{
    public class Inventory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("product_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        [BsonElement("vendor_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VendorId { get; set; }

        [BsonElement("stock_quantity")]
        public int StockQuantity { get; set; }

        [BsonElement("low_stock_alert")]
        public bool LowStockAlert { get; set; }

        [BsonElement("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}
