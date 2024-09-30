using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class OrderService : IOrderService
    {
        private readonly IMongoCollection<Order> _ordersCollection;
        private readonly IMongoCollection<Product> _productsCollection;
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;

        public OrderService(IOptions<DatabaseSettings> dbSettings,
                            IProductService productService,
                            IInventoryService inventoryService)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _ordersCollection = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
            _productsCollection = database.GetCollection<Product>(dbSettings.Value.ProductCollectionName);
            _productService = productService;
            _inventoryService = inventoryService;
        }

        public async Task<Order> PlaceOrderAsync(PlaceOrderDto placeOrderDto)
        {
            // Validate CustomerId
            if (!ObjectId.TryParse(placeOrderDto.CustomerId, out _))
            {
                throw new ArgumentException("Invalid CustomerId.");
            }

            var orderItems = new List<OrderItem>();
            double totalAmount = 0.0;
            var vendorStatusDict = new Dictionary<string, VendorOrderStatus>();

            foreach (var itemDto in placeOrderDto.OrderItems)
            {
                // Validate ProductId
                if (!ObjectId.TryParse(itemDto.ProductId, out var productIdObj))
                {
                    throw new ArgumentException($"Invalid ProductId: {itemDto.ProductId}");
                }

                var product = _productService.GetProductById(itemDto.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product not found: {itemDto.ProductId}");
                }

                if (itemDto.Quantity <= 0)
                {
                    throw new ArgumentException("Quantity must be greater than zero.");
                }

                // Check inventory
                var inventory = await _inventoryService.GetInventoryByProductIdAsync(itemDto.ProductId);
                if (inventory == null || inventory.StockQuantity < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                }

                // Deduct stock
                /*inventory.StockQuantity -= itemDto.Quantity;
                await _inventoryService.UpdateInventoryAsync(inventory);*/

                // Calculate total
                totalAmount += product.Price * itemDto.Quantity;

                // Prepare order item
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    Price = product.Price
                };
                orderItems.Add(orderItem);

                // Update vendor status
                if (!vendorStatusDict.ContainsKey(product.VendorId))
                {
                    vendorStatusDict[product.VendorId] = new VendorOrderStatus
                    {
                        VendorId = product.VendorId,
                        Status = "Processing"
                    };
                }
            }

            var order = new Order
            {
                CustomerId = placeOrderDto.CustomerId,
                OrderItems = orderItems,
                TotalAmount = totalAmount,
                ShippingAddress = new Address
                {
                    Street = placeOrderDto.ShippingAddress.Street,
                    City = placeOrderDto.ShippingAddress.City,
                    ZipCode = placeOrderDto.ShippingAddress.ZipCode
                },
                PlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                VendorStatus = new List<VendorOrderStatus>(vendorStatusDict.Values)
            };

            await _ordersCollection.InsertOneAsync(order);
            return order;
        }


        /// <summary>
        /// Cancels an existing order if it is in the "Processing" status.
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <returns>The updated order with status set to "Cancelled".</returns>
        /// <exception cref="ArgumentException">Thrown if the order is not in a cancellable status.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        /*public async Task<Order> CancelOrderAsync(string orderId)
        {
            Order order = await _ordersCollection.Find(Order => Order.Id == orderId).FirstOrDefaultAsync();
            if(order != null && !(order.OrderStatus == "Dispatched" || order.OrderStatus == "Delivered"))
            {
                var update = Builders<Order>.Update
                                            .Set(o => o.OrderStatus, "Cancelled")
                                            .Set(o => o.UpdatedAt, DateTime.UtcNow);
                order.OrderStatus = "Cancelled";
                var result = await _ordersCollection.UpdateOneAsync(Order => Order.Id == orderId, update);
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Failed to update the order status.");
                }
                // Retrieve the updated order
                order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
                return order;
            }
            else if (order != null)
            {
                throw new ArgumentException($"Order is already in {order.OrderStatus} status, cannot be cancelled.");
            }
            else
            {
                throw new ArgumentException($"Order not found.");
            }
        }*/


        /// <summary>
        /// Initiates a cancellation request for an order.
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <returns>The updated order with status set to "Cancellation Requested".</returns>
        /// <exception cref="ArgumentException">Thrown if the order is not in a cancellable status.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        public async Task<Order> RequestCancelOrderAsync(string orderId)
        {
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null && !(order.OrderStatus == "Dispatched" || order.OrderStatus == "Delivered"))
            {
                var update = Builders<Order>.Update
                                            .Set(o => o.OrderStatus, "Cancellation Requested")
                                            .Set(o => o.UpdatedAt, DateTime.UtcNow);
                var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, update);
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Failed to update the order status.");
                }

                // Retrieve the updated order
                order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
                return order;
            }
            else if (order != null)
            {
                throw new ArgumentException($"Order is already in {order.OrderStatus} status and cannot be cancelled.");
            }
            else
            {
                throw new KeyNotFoundException($"Order not found.");
            }
        }


        /// <summary>
        /// Confirms the cancellation of an order if a cancellation request was made.
        /// </summary>
        /// <param name="orderId">The ID of the order to confirm cancellation.</param>
        /// <returns>The updated order with status set to "Cancelled".</returns>
        /// <exception cref="ArgumentException">Thrown if the order is not in the 'Cancellation Requested' status.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        public async Task<Order> ConfirmCancelOrderAsync(string orderId)
        {
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null && order.OrderStatus == "Cancellation Requested")
            {
                var update = Builders<Order>.Update
                                            .Set(o => o.OrderStatus, "Cancelled")
                                            .Set(o => o.UpdatedAt, DateTime.UtcNow);
                var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, update);
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Failed to update the order status.");
                }

                // Retrieve the updated order
                order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
                return order;
            }
            else if (order != null)
            {
                throw new ArgumentException($"Order is not in 'Cancellation Requested' status and cannot be cancelled.");
            }
            else
            {
                throw new KeyNotFoundException($"Order not found.");
            }
        }



        public async Task<Order> UpdateOrderAsync(string orderId, UpdateOrderDto updateOrderDto)
        {
            // Validate OrderId
            if (!ObjectId.TryParse(orderId, out _))
            {
                throw new ArgumentException("Invalid OrderId.");
            }

            // Find the existing order
            var existingOrder = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (existingOrder == null)
            {
                throw new ArgumentException("Order not found.");
            }

            // Update order items if provided
            if (updateOrderDto.OrderItems != null && updateOrderDto.OrderItems.Count > 0)
            {
                var updatedOrderItems = new List<OrderItem>();
                double updatedTotalAmount = 0.0;

                foreach (var itemDto in updateOrderDto.OrderItems)
                {
                    if (!ObjectId.TryParse(itemDto.ProductId, out var productIdObj))
                    {
                        throw new ArgumentException($"Invalid ProductId: {itemDto.ProductId}");
                    }

                    var product = _productService.GetProductById(itemDto.ProductId);
                    if (product == null)
                    {
                        throw new ArgumentException($"Product not found: {itemDto.ProductId}");
                    }

                    if (itemDto.Quantity <= 0)
                    {
                        throw new ArgumentException("Quantity must be greater than zero.");
                    }

                    // Check inventory
                    var inventory = await _inventoryService.GetInventoryByProductIdAsync(itemDto.ProductId);
                    if (inventory == null || inventory.StockQuantity < itemDto.Quantity)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                    }

                    // Calculate total
                    updatedTotalAmount += product.Price * itemDto.Quantity;

                    // Prepare updated order item
                    var updatedOrderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = itemDto.Quantity,
                        Price = product.Price
                    };
                    updatedOrderItems.Add(updatedOrderItem);
                }

                // Update the existing order object
                existingOrder.OrderItems = updatedOrderItems;
                existingOrder.TotalAmount = updatedTotalAmount;
            }

            // Update the shipping address if provided
            if (updateOrderDto.ShippingAddress != null)
            {
                existingOrder.ShippingAddress = new Address
                {
                    Street = updateOrderDto.ShippingAddress.Street,
                    City = updateOrderDto.ShippingAddress.City,
                    ZipCode = updateOrderDto.ShippingAddress.ZipCode
                };
            }

            // Update the note if provided
            /*if (!string.IsNullOrEmpty(updateOrderDto.Note))
            {
                existingOrder.Note = updateOrderDto.Note;
            }*/

            // Update the UpdatedAt field
            existingOrder.UpdatedAt = DateTime.UtcNow;

            // Update the order in the database
            var updateResult = await _ordersCollection.ReplaceOneAsync(o => o.Id == orderId, existingOrder);
            if (updateResult.ModifiedCount == 0)
            {
                throw new Exception("Failed to update the order.");
            }

            return existingOrder;
        }




        public async Task<Order> UpdateVendorOrderStatusAsync(string orderId, string vendorId, string status)
        {
            // Find the order
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }

            // Find the vendor status in the order
            var vendorStatus = order.VendorStatus.FirstOrDefault(vs => vs.VendorId == vendorId);
            if (vendorStatus == null)
            {
                throw new ArgumentException("Vendor not associated with this order.");
            }

            // Update the vendor status
            vendorStatus.Status = status;

            // Update the order status
            if (order.VendorStatus.All(vs => vs.Status == "Delivered"))
            {
                order.OrderStatus = "Delivered";
            }
            else if (order.VendorStatus.Any(vs => vs.Status == "Delivered"))
            {
                order.OrderStatus = "Partially Delivered";
            }

            // Update the order in the database
            var update = Builders<Order>.Update
                .Set(o => o.VendorStatus, order.VendorStatus)
                .Set(o => o.OrderStatus, order.OrderStatus)
                .Set(o => o.UpdatedAt, DateTime.UtcNow);

            var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Failed to update the vendor status.");
            }

            // Retrieve the updated order
            order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            return order;
        }




        /// <summary>
        /// Retrieves a specific order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID '{orderId}' not found.");
            }
            return order;
        }


        /// <summary>
        /// Retrieves all orders with optional pagination.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of orders per page.</param>
        /// <returns>A list of orders.</returns>
        public async Task<List<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _ordersCollection
                .Find(_ => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }


        public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId, int pageNumber = 1, int pageSize = 10)
        {
            // Get all orders that contain items from the vendor
            var orders = await _ordersCollection
                .Find(order => order.VendorStatus.Any(vs => vs.VendorId == vendorId))
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Filter out order items that are not from the vendor
            foreach (var order in orders)
            {
                order.OrderItems = order.OrderItems
                    .Where(item => IsProductFromVendor(item.ProductId, vendorId))
                    .ToList();
            }

            return orders.Where(order => order.OrderItems.Any()).ToList(); // Only return orders that have items from the vendor
        }

        private bool IsProductFromVendor(string productId, string vendorId)
        {
            // This is a placeholder function. Replace with actual MongoDB query logic to check product's vendor.
            var product = _productsCollection.Find(p => p.Id == productId).FirstOrDefault();
            return product != null && product.VendorId == vendorId;
        }


    }
}
