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
        // MongoDB collections
        private readonly IMongoCollection<Order> _ordersCollection;
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Product> _productsCollection;
        // Services dependencies
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;
        private readonly INotificationService _notificationService;

        public OrderService(IOptions<DatabaseSettings> dbSettings,
                            IProductService productService,
                            IInventoryService inventoryService, INotificationService notificationService)
        {
            // Set up MongoDB client and retrieve necessary collections
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _usersCollection = database.GetCollection<User>(dbSettings.Value.UserCollectionName);
            _ordersCollection = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
            _productsCollection = database.GetCollection<Product>(dbSettings.Value.ProductCollectionName);
            
            // Assign service dependencies
            _productService = productService;
            _inventoryService = inventoryService;
            _notificationService = notificationService;
        }


        /// <summary>
        /// Places an order based on provided order details.
        /// </summary>
        /// <param name="placeOrderDto">The details of the order to be placed.</param>
        /// <returns>The placed order.</returns>
        /// <exception cref="ArgumentException">Thrown if the customer ID or product ID is invalid, or if the quantity is less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is insufficient stock for any product.</exception>
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
                        Status = Constant.PROCESSING
                    };
                }
            }

            // Generate a unique OrderID
            var orderID = await GenerateUniqueOrderIDAsync();

            var order = new Order
            {
                CustomerId = placeOrderDto.CustomerId,
                OrderID = orderID,
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
        /// Initiates a cancellation request for an order.
        /// </summary>
        /// <param name="orderId">The ID of the order to cancel.</param>
        /// <returns>The updated order with status set to "Cancellation Requested".</returns>
        /// <exception cref="ArgumentException">Thrown if the order is not in a cancellable status.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        public async Task<Order> RequestCancelOrderAsync(string orderId)
        {
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null && !(order.OrderStatus == Constant.DISPATCHED || order.OrderStatus == Constant.DELIVERED))
            {
                var update = Builders<Order>.Update
                                            .Set(o => o.OrderStatus, Constant.CANCEL_REQUESTED)
                                            .Set(o => o.UpdatedAt, DateTime.UtcNow);
                var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, update);
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Failed to update the order status.");
                }
                //***notification***//
                if (_notificationService != null)
                {
                    var adminUsers = await _usersCollection.Find(u => u.UserType == Constant.ADMIN).ToListAsync();
                                      
                    foreach (var admin in adminUsers)
                    {
                        await _notificationService.CreateNotification(new Notification
                        {
                            UserId = admin.Id,
                            Message = $"Order Id :{order.Id} has a cancellation request."
                        });
                    }
                    var csrUsers = await _usersCollection.Find(u => u.UserType == Constant.CSR).ToListAsync();

                    foreach (var csr in csrUsers)
                    {
                        await _notificationService.CreateNotification(new Notification
                        {
                            UserId = csr.Id,
                            Message = $"Order Id :{order.Id} has a cancellation request."
                        });
                    }
                }
                else { throw new InvalidOperationException("Notification service is not initialized.");}

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
            if (order != null && order.OrderStatus == Constant.CANCEL_REQUESTED)
            {
                var update = Builders<Order>.Update
                                            .Set(o => o.OrderStatus, Constant.CANCELLED)
                                            .Set(o => o.UpdatedAt, DateTime.UtcNow);
                var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, update);
                if (result.ModifiedCount == 0)
                {
                    throw new Exception("Failed to update the order status.");
                }

                // Retrieve the updated order
                order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();

                ////***notification***//
                if (_notificationService != null)
                {
                   await _notificationService.CreateNotification(new Notification
                   {
                       UserId = order.CustomerId,
                       Message = $"Order Id :{order.Id} has been cancelled."
                   });
                }
                else { throw new InvalidOperationException("Notification service is not initialized.");}
                
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



        /// <summary>
        /// Updates an existing order based on provided update details.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="updateOrderDto">The details of the order to be updated.</param>
        /// <returns>The updated order.</returns>
        /// <exception cref="ArgumentException">Thrown if the order or product ID is invalid or if the order is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is insufficient stock for any product.</exception>
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



        /// <summary>
        /// Updates the status of an order to "Dispatched" and adjusts inventory accordingly.
        /// </summary>
        /// <param name="orderId">The ID of the order to be dispatched.</param>
        /// <returns>The updated order after dispatch.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown if the order is not in a "Pending" status.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is insufficient stock for any product in the order.</exception>
        /// <exception cref="Exception">Thrown if the order status update fails.</exception>
        public async Task<Order> DispatchOrderStatusAsync(string orderId)
        {
            // Find the order
            var order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order == null)
            {
                throw new KeyNotFoundException("Order not found.");
            }


            // Find the status in the order
            var orderStatus = order.OrderStatus;
            if (orderStatus != Constant.PENDING)
            {
                throw new ArgumentException("Only Pending orders can be marked as Dispatched");
            }

            // Update the order status to "Dispatched"
            var updateDefinition = Builders<Order>.Update.Set(o => o.OrderStatus, Constant.DISPATCHED);
            var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, updateDefinition);

            // Update the order status
            //dispatchingOrder.OrderStatus = "Dispatched";

            //var result = await _ordersCollection.UpdateOneAsync(o => o.Id == orderId, dispatchingOrder);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Failed to update the vendor status.");
            }


            foreach (var item in order.OrderItems)
            {
                // Validate ProductId
                if (!ObjectId.TryParse(item.ProductId, out var productIdObj))
                {
                    throw new ArgumentException($"Invalid ProductId: {item.ProductId}");
                }

                var product = _productService.GetProductById(item.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product not found: {item.ProductId}");
                }

                // Check inventory
                var inventory = await _inventoryService.GetInventoryByProductIdAsync(item.ProductId);
                if (inventory == null || inventory.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {product.Name}");
                }

                // Deduct stock
                inventory.StockQuantity -= item.Quantity;
                await _inventoryService.UpdateInventoryAsync(inventory);
            }

            // Retrieve the updated order
            order = await _ordersCollection.Find(o => o.Id == orderId).FirstOrDefaultAsync();
            return order;
        }




        /// <summary>
        /// Updates the vendor-specific status of an order and adjusts the overall order status accordingly.
        /// Also sends a notification to the customer if the order is fully delivered.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="vendorId">The ID of the vendor whose status is being updated.</param>
        /// <returns>The updated order after vendor status update.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the order does not exist.</exception>
        /// <exception cref="ArgumentException">Thrown if the vendor is not associated with the order.</exception>
        /// <exception cref="Exception">Thrown if the vendor status update fails.</exception>
        public async Task<Order> UpdateVendorOrderStatusAsync(string orderId, string vendorId)
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
            vendorStatus.Status = Constant.DELIVERED;

            // Update the order status
            if (order.VendorStatus.All(vs => vs.Status == Constant.DELIVERED))
            {
                order.OrderStatus = Constant.DELIVERED;
                //***notification***//
                if (_notificationService != null)
                {
                   await _notificationService.CreateNotification(new Notification
                   {
                       UserId = order.CustomerId,
                       Message = $"Order Id :{order.Id} has been delivered."
                   });
                }
            }
            else if (order.VendorStatus.Any(vs => vs.Status == Constant.DELIVERED))
            {
                order.OrderStatus = Constant.PARTIALLY_DELIVERED;
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

            // Fetch customer name
            var customer = await _usersCollection.Find(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
            if (customer != null)
            {
                order.CustomerName = customer.Name;
            }

            // Fetch vendor names
            foreach (var vendorStatus in order.VendorStatus)
            {
                var vendor = await _usersCollection.Find(u => u.Id == vendorStatus.VendorId).FirstOrDefaultAsync();
                if (vendor != null)
                {
                    vendorStatus.VendorName = vendor.Name;
                }
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
            var orders = await _ordersCollection
                .Find(_ => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            foreach (var order in orders)
            {
                // Fetch customer name
                var customer = await _usersCollection.Find(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerName = customer.Name;
                }

                // Fetch vendor names
                foreach (var vendorStatus in order.VendorStatus)
                {
                    var vendor = await _usersCollection.Find(u => u.Id == vendorStatus.VendorId).FirstOrDefaultAsync();
                    if (vendor != null)
                    {
                        vendorStatus.VendorName = vendor.Name;
                    }
                }
            }

            return orders;
        }



        /// <summary>
        /// Retrieves all orders by customer ID with optional pagination.
        /// </summary>
        /// <param name="customerId">The ID of the customer to retrieve orders for.</param>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of orders per page.</param>
        /// <returns>A list of orders associated with the customer.</returns>
        public async Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId, int pageNumber = 1, int pageSize = 10)
        {
            var orders = await _ordersCollection
                .Find(order => order.CustomerId == customerId)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Filter out order items that are not from the vendor
            foreach (var order in orders)
            {
                // Fetch customer name
                var customer = await _usersCollection.Find(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerName = customer.Name;
                }

                foreach (var vendorStatus in order.VendorStatus)
                {
                    var vendor = await _usersCollection.Find(u => u.Id == vendorStatus.VendorId).FirstOrDefaultAsync();
                    if (vendor != null)
                    {
                        vendorStatus.VendorName = vendor.Name;
                    }
                }
            }

            return orders;
        }



        /// <summary>
        /// Retrieves all orders that include items from a specific vendor, with optional pagination.
        /// </summary>
        /// <param name="vendorId">The ID of the vendor whose orders are to be retrieved.</param>
        /// <param name="pageNumber">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of orders per page (default is 10).</param>
        /// <returns>A list of orders containing items from the specified vendor.</returns>
        /// <exception cref="ArgumentException">Thrown if the vendor ID is invalid.</exception>
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
                // Fetch customer name
                var customer = await _usersCollection.Find(u => u.Id == order.CustomerId).FirstOrDefaultAsync();
                if (customer != null)
                {
                    order.CustomerName = customer.Name;
                }

                order.OrderItems = order.OrderItems
                    .Where(item => IsProductFromVendor(item.ProductId, vendorId))
                    .ToList();

                foreach (var vendorStatus in order.VendorStatus)
                {
                    var vendor = await _usersCollection.Find(u => u.Id == vendorStatus.VendorId).FirstOrDefaultAsync();
                    if (vendor != null)
                    {
                        vendorStatus.VendorName = vendor.Name;
                    }
                }
            }

            return orders.Where(order => order.OrderItems.Any()).ToList(); // Only return orders that have items from the vendor
        }





        private bool IsProductFromVendor(string productId, string vendorId)
        {
            // MongoDB query logic to check product's vendor.
            var product = _productsCollection.Find(p => p.Id == productId).FirstOrDefault();
            return product != null && product.VendorId == vendorId;
        }



        // Method to generate a unique order ID in the format "EC-[<8-digit number>]"
        private async Task<string> GenerateUniqueOrderIDAsync()
        {
            string orderID;
            bool isUnique = false;

            do
            {
                // Generate an 8-digit number
                var randomNumber = new Random().Next(10000000, 99999999);
                orderID = $"EC-{randomNumber}";

                // Check if the generated OrderID is unique
                var existingOrder = await _ordersCollection.Find(o => o.OrderID == orderID).FirstOrDefaultAsync();
                if (existingOrder == null)
                {
                    isUnique = true;
                }
            } while (!isUnique);

            return orderID;
        }


    }
}
