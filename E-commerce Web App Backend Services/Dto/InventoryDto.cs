using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class InventoryDto
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string ProductId { get; set; }

        public int StockQuantity { get; set; }
    }
}
