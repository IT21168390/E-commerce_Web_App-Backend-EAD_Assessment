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
        private readonly IProductService _productService;
        private readonly IInventoryService _inventoryService;

        public OrderService(IOptions<DatabaseSettings> dbSettings,
                            IProductService productService,
                            IInventoryService inventoryService)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _ordersCollection = database.GetCollection<Order>(dbSettings.Value.OrdersCollectionName);
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
                inventory.StockQuantity -= itemDto.Quantity;
                await _inventoryService.UpdateInventoryAsync(inventory);

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
    }
}
