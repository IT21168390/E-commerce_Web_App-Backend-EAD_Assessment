using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.Dto;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    /// <summary>
    /// Service for managing inventory operations.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly INotificationService _notificationService;
        private readonly IMongoCollection<Product> _productCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryService"/> class.
        /// </summary>
        /// <param name="dbSettings">The database settings.</param>
        /// <param name="_notificationService">The notification service.</param>
        public InventoryService(IOptions<DatabaseSettings> dbSettings, INotificationService _notificationService)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _inventoryCollection = database.GetCollection<Inventory>(dbSettings.Value.InventoryCollectionName);
            _orderCollection = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
            _productCollection = database.GetCollection<Product>(dbSettings.Value.ProductCollectionName);
            this._notificationService = _notificationService;
        }

        /// <summary>
        /// Gets all inventory items.
        /// </summary>
        /// <returns>A list of all inventory items.</returns>
        public async Task<IEnumerable<Inventory>> GetAllInventory()
        {
            return await _inventoryCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Gets a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <returns>The inventory item with the specified ID.</returns>
        public async Task<Inventory> GetInventoryById(string id)
        {
            return await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new inventory item.
        /// </summary>
        /// <param name="inventory">The inventory item to create.</param>
        public async Task CreateInventory(Inventory inventory)
        {
            inventory.LastUpdated = DateTime.UtcNow;
            await _inventoryCollection.InsertOneAsync(inventory);
        }

        /// <summary>
        /// Updates a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to update.</param>
        /// <param name="updatedInventory">The updated inventory data.</param>
        /// <returns>The updated inventory item.</returns>
        public async Task<Inventory> UpdateInventory(string id, InventoryUpdateDto updatedInventory)
        {
            var existingInventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
            if (existingInventory != null)
            {
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

        /// <summary>
        /// Updates the inventory item for a purchase by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to update.</param>
        /// <param name="updatedInventory">The updated inventory data for the purchase.</param>
        /// <returns>The updated inventory item.</returns>
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

        /// <summary>
        /// Deletes a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to delete.</param>
        /// <returns>True if the deletion is successful, otherwise false.</returns>
        public async Task<bool> DeleteInventory(string id)
        {
            // Get the product ID from the inventory collection
            var product_id = await _inventoryCollection.Find(i => i.Id == id).Project(i => i.ProductId).FirstOrDefaultAsync();
            // Add business logic to check for pending orders here before deleting
            var inventory = await GetInventoryById(id);
            var hasPendingOrder = await HasPendingOrders(product_id);
            if (inventory == null || hasPendingOrder)
            {
                return false;
            }
            var result = await _inventoryCollection.DeleteOneAsync(i => i.Id == id);

            // When deleting the inventory record of a product, delete the associated product as well
            if (result.DeletedCount > 0)
            {
                if (!string.IsNullOrEmpty(product_id))
                {
                    var productDeleteResult = await _productCollection.DeleteOneAsync(p => p.Id == product_id);
                    if (productDeleteResult == null)
                    {
                        // Handle the case where the product deletion failed
                        throw new InvalidOperationException($"Failed to delete the product with id {product_id}");
                    }
                    else
                    {
                        // Handle the case where the product deletion succeeded
                        Console.WriteLine($"Product with id {product_id} deleted successfully");
                    }
                }
            }

            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Checks if there are any pending orders for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product to check for pending orders.</param>
        /// <returns>True if there are pending orders, otherwise false.</returns>
        private async Task<bool> HasPendingOrders(string productId)
        {
            // Query the orders collection for any orders with pending status that contain the product
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.OrderStatus, "Pending"),
                Builders<Order>.Filter.ElemMatch(o => o.OrderItems, Builders<OrderItem>.Filter.Eq(oi => oi.ProductId, productId))
            );

            if (_orderCollection != null)
            {
                var pendingOrder = await _orderCollection.Find(filter).FirstOrDefaultAsync();
                return pendingOrder != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks for low stock and sends a notification to the vendor.
        /// </summary>
        /// <param name="vendorId">The ID of the vendor to notify.</param>
        /// <returns>True if the notification is sent successfully, otherwise false.</returns>
        public async Task<bool> CheckLowStockAndNotify(string vendorId)
        {
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
            }
            else
            {
                throw new InvalidOperationException("Notification service is not initialized.");
            }

            return true;
        }

        /// <summary>
        /// Gets a specific inventory item by product ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The inventory item with the specified product ID.</returns>
        public async Task<Inventory> GetInventoryByProductIdAsync(string productId)
        {
            if (!ObjectId.TryParse(productId, out var objectId))
            {
                return null;
            }

            return await _inventoryCollection.Find(i => i.ProductId == productId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates an inventory item.
        /// </summary>
        /// <param name="inventory">The inventory item to update.</param>
        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            await _inventoryCollection.ReplaceOneAsync(i => i.Id == inventory.Id, inventory);
        }
    }
}