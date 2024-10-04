using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class InventoryUpdateDto
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string ProductId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string VendorId { get; set; }
        
        public int StockQuantity { get; set; }
    }
}
