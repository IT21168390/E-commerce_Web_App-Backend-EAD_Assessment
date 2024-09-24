namespace E_commerce_Web_App_Backend_Services.models
{
    public interface IDatabaseSettings
    {
        string UserCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
