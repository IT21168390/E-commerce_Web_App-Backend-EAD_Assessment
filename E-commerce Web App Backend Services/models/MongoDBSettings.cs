namespace E_commerce_Web_App_Backend_Services.models
{
    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UserCollectionName { get; set; } = null!;
        public string ProductCollectionName { get; set; } = null!;
    }
}
