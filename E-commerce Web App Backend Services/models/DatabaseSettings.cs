﻿namespace E_commerce_Web_App_Backend_Services.models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UserCollectionName { get; set; }
        public string ProductCollectionName { get; set; }
    }
}
