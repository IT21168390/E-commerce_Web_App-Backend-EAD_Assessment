using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.Dto;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class InventoryService : IInventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly INotificationService _notificationService;

        public InventoryService(IOptions<DatabaseSettings> dbSettings, INotificationService _notificationService)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(dbSettings.Value.InventoryCollectionName);
            _orderCollection = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
            this._notificationService = _notificationService;
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

         public async Task<Inventory> UpdateInventory(string id, InventoryUpdateDto updatedInventory)
        {

            var existingInventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
            if (existingInventory != null) {
                var newInventory = new Inventory
                {
                    Id = id,
                    ProductId = updatedInventory.ProductId,
                    StockQuantity = updatedInventory.StockQuantity,
                    VendorId = existingInventory.VendorId,
                    LastUpdated = DateTime.UtcNow
                };
                if (updatedInventory.StockQuantity < 0)
                {
                    throw new ArgumentException("Stock quantity cannot be negative!");
                }

                if (newInventory.StockQuantity < 10)
                {
                    newInventory.LowStockAlert = true;
                    await CheckLowStockAndNotify(updatedInventory.VendorId);

                }
                else
                {
                    newInventory.LowStockAlert = false;
                }


                var result = await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, newInventory);

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    return newInventory;

                }
            }
            else
            {
                throw new ArgumentException("Inventory not found!");
            }         
            return null;
        }

        public async Task<Inventory> UpdateInventoryBuy(string id, InventoryDto updatedInventory)
        {
            var existingInventory = await _inventoryCollection.Find(o => o.Id == id).FirstOrDefaultAsync();

            if (existingInventory.StockQuantity - updatedInventory.StockQuantity < 0)
            {
                throw new ArgumentException("Not having sufficient quantity!");
            }

            var newInventory = new Inventory
            {
                Id = id,
                ProductId = updatedInventory.ProductId,
                StockQuantity = existingInventory.StockQuantity - updatedInventory.StockQuantity,
                VendorId = existingInventory.VendorId,
                LastUpdated = DateTime.UtcNow
            };
            var result = await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, newInventory);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                // Check if low stock alert is needed
                if (newInventory.StockQuantity < 10)
                {
                    newInventory.LowStockAlert = true;
                    await CheckLowStockAndNotify(newInventory.VendorId);
                }
                return newInventory;
            }
            return null;
        }

        public async Task<bool> DeleteInventory(string id)
        {
            //get the product id from the inventory collection
            var product_id = await _inventoryCollection.Find(i => i.Id == id).Project(i => i.ProductId).FirstOrDefaultAsync();
            // Add business logic to check for pending orders here before deleting
            var inventory = await GetInventoryById(id);
            var hasPendingOrder = await HasPendingOrders(product_id);
            if (inventory == null || hasPendingOrder)
            {
                return false;
            }
            var result = await _inventoryCollection.DeleteOneAsync(i => i.Id == id);
            return result.DeletedCount > 0;
        }

        private async Task<bool> HasPendingOrders(string productId)
        {
            // Query the orders collection for any orders with pending status that contain the product
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.OrderStatus, "Pending"),
                Builders<Order>.Filter.ElemMatch(o => o.OrderItems, Builders<OrderItem>.Filter.Eq(oi => oi.ProductId, productId))
            );

            if(_orderCollection != null)
            {
                  var pendingOrder = await _orderCollection.Find(filter).FirstOrDefaultAsync();
                  return pendingOrder != null;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> CheckLowStockAndNotify(string vendorId)
        {
            //***notification***//
            if (_notificationService != null)
            {
                object value = await _notificationService.CreateNotification(new Notification
                {
                    UserId = vendorId,
                    Message = "Low stock alert",
                });

                if (value != null)
                {
                    Console.WriteLine($"Notification sent to Vendor {vendorId} for low stock");
                }
            }else
            {
                throw new InvalidOperationException("Notification service is not initialized.");
            }
            
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