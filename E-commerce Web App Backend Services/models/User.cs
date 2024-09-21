using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace E_commerce_Web_App_Backend_Services
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("user_type")]
        public string UserType { get; set; } // "Administrator", "Vendor", "CSR", "Customer"

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // "active", "deactivated"

        [BsonElement("account_created_at")]
        public DateTime AccountCreatedAt { get; set; }

        // Embedded document for Vendor details
        [BsonElement("vendor_details")]
        public VendorDetails VendorDetails { get; set; }

        // Embedded document for Customer details
        [BsonElement("customer_details")]
        public CustomerDetails CustomerDetails { get; set; }
    }

    public class VendorDetails
    {
        [BsonElement("company_name")]
        public string CompanyName { get; set; }

        [BsonElement("vendor_rating")]
        public double VendorRating { get; set; }

        [BsonElement("products")]
        public List<VendorProduct> Products { get; set; }
    }

    public class VendorProduct
    {
        [BsonElement("product_id")]
        public string ProductId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("rating")]
        public double Rating { get; set; }
    }

    public class CustomerDetails
    {
        [BsonElement("shipping_address")]
        public Address ShippingAddress { get; set; }

        [BsonElement("order_history")]
        public List<OrderHistory> OrderHistory { get; set; }
    }

    public class OrderHistory
    {
        [BsonElement("order_id")]
        public string OrderId { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // "delivered", "processing", etc.

        [BsonElement("total_amount")]
        public double TotalAmount { get; set; }
    }

    public class Address
    {
        [BsonElement("street")]
        public string Street { get; set; }

        [BsonElement("city")]
        public string City { get; set; }

        [BsonElement("zip_code")]
        public string ZipCode { get; set; }
    }
}
