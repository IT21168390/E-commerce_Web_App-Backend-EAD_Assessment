using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class InventoryService : IInventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;

        public InventoryService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(dbSettings.Value.InventoryCollectionName);
        }

        public async Task<IEnumerable<Inventory>> GetAllInventory()
        {
            return await _inventoryCollection.Find(_ => true).ToListAsync();
        }



        public async Task<Inventory> GetInventoryById(string id)
        {
            return await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateInventory(Inventory inventory)
        {
            inventory.LastUpdated = DateTime.UtcNow;
            await _inventoryCollection.InsertOneAsync(inventory);
        }

        public async Task<Inventory> UpdateInventory(string id, Inventory updatedInventory)
        {
            updatedInventory.LastUpdated = DateTime.UtcNow;
            var result = await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, updatedInventory);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                // Check if low stock alert is needed
                if (updatedInventory.StockQuantity < 10)
                {
                    await CheckLowStockAndNotify(updatedInventory.VendorId);
                }
                return updatedInventory;
            }
            return null;
        }

        public async Task<bool> DeleteInventory(string id)
        {
            // Add business logic to check for pending orders here before deleting
            var inventory = await GetInventoryById(id);
            if (inventory == null || HasPendingOrders(id))
            {
                return false;
            }
            var result = await _inventoryCollection.DeleteOneAsync(i => i.Id == id);
            return result.DeletedCount > 0;
        }

        private bool HasPendingOrders(string productId)
        {
            // Logic to check pending orders from the orders collection
            return false;
        }

        public async Task<bool> CheckLowStockAndNotify(string vendorId)
        {
            // Logic to notify the vendor
            Console.WriteLine($"Notification sent to Vendor {vendorId} for low stock");
            return true;
        }

        public async Task<Inventory> GetInventoryByProductIdAsync(string productId)
        {
            if (!ObjectId.TryParse(productId, out var objectId))
            {
                return null;
            }

            return await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            await _inventoryCollection.ReplaceOneAsync(i => i.Id == inventory.Id, inventory);
        }
    }
}