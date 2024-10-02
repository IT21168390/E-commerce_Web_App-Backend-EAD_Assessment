namespace E_commerce_Web_App_Backend_Services.models
{
    public interface IDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string UserCollectionName { get; set; }
        string ProductCollectionName { get; set; }
        string InventoryCollectionName { get; set; }
        string OrdersCollectionName { get; set; }
        string NotificationCollectionName { get; set; }
        string VendorRatingCollectionName { get; set; }
    }
}
