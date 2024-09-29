using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_type")]
        public string? UserType { get; set; } // "Administrator", "Vendor", "CSR", "Customer"

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("password")]
        public string? Password { get; set; }

        [BsonElement("status")]
        public string? Status { get; set; } // "active", "deactivated"

        [BsonElement("account_created_at")]
        public DateTime? AccountCreatedAt { get; set; }

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
