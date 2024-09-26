using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("order_id")]
        public string OrderId { get; set; }

        [BsonElement("customer_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string CustomerId { get; set; }

        [BsonElement("order_status")]
        public string OrderStatus { get; set; } // "processing", "delivered", "cancelled", etc.

        [BsonElement("order_items")]
        public List<OrderItem> OrderItems { get; set; }

        [BsonElement("total_amount")]
        public double TotalAmount { get; set; }

        [BsonElement("shipping_address")]
        public Address ShippingAddress { get; set; }

        [BsonElement("placed_at")]
        public DateTime PlacedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("vendor_status")]
        public List<VendorOrderStatus> VendorStatus { get; set; }
    }

    public class OrderItem
    {
        [BsonElement("product_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string ProductId { get; set; }

        [BsonElement("product_name")]
        public string ProductName { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }
    }

    public class VendorOrderStatus
    {
        [BsonElement("vendor_id")]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string VendorId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // "ready", "partially delivered", etc.
    }
}
