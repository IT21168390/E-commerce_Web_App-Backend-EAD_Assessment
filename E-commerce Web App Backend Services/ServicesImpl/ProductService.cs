using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Inventory> _inventory;
        private readonly IMongoCollection<User> _user;

        // Constructor to initialize the ProductService with MongoDB collections for products, inventory, and users
        public ProductService(IDatabaseSettings settings, IMongoClient mongoClient) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.ProductCollectionName);
            _inventory = database.GetCollection<Inventory>(settings.InventoryCollectionName);
            _user = database.GetCollection<User>(settings.UserCollectionName);
        }

        // Adds a new product to the database along with its inventory details
        public Product AddProduct(ProductDTO newProduct)
        {
            // Create a Product object from the ProductDTO
            Product product = new Product {
                ProductId = newProduct.ProductId,
                Name = newProduct.Name, 
                Category = newProduct.Category, 
                VendorId = newProduct.VendorId, 
                Description = newProduct.Description, 
                Price = newProduct.Price, 
                Status = newProduct.Status, 
                CreatedAt = newProduct.CreatedAt };

            // Insert the product into the products collection
            _products.InsertOne(product);
            // Create and insert a new inventory entry for the product
            _inventory.InsertOne(
                new Inventory {
                    ProductId = product.Id, 
                    VendorId = product.VendorId, 
                    StockQuantity = newProduct.stockQuantity , 
                    LastUpdated = DateTime.Now, 
                    LowStockAlert = false 
                });

            return product;
        }


        // Retrieves a list of all products with their associated vendor names and inventory quantities
        public List<ProductDTO> GetAllProductList()
        {
            // Get all products from the database
            var products = _products.Find(p => true).ToList();

            // Get a list of distinct vendor IDs from the products
            var vendorIds = products.Select(p => p.VendorId).Distinct().ToList();
            var vendors = _user.Find(u => vendorIds.Contains(u.Id)).ToList()
                .ToDictionary(u => u.Id, u => u.Name); // Create a dictionary for fast lookups

            // Create a list of ProductDTOs with VendorName and inventory stockQuantity populated
            var productDtos = products.Select(product => new ProductDTO
            {
                Id = product.Id,
                ProductId = product.ProductId,
                Name = product.Name,
                Category = product.Category,
                VendorId = product.VendorId,
                Description = product.Description,
                Price = product.Price,
                Status = product.Status,
                CreatedAt = product.CreatedAt,
                stockQuantity = _inventory.Find(i => i.ProductId == product.Id).FirstOrDefault()?.StockQuantity ?? 0,
                VendorName = vendors.ContainsKey(product.VendorId) ? vendors[product.VendorId] : null // Populate VendorName
            }).ToList();

            return productDtos;
        }


        // Retrieves a product by its ID
        public Product GetProductById(string id)
        {
            return _products.Find(Product => Product.Id == id).FirstOrDefault();
        }

        // Removes a product from the database by its ID
        public void RemoveProductById(string id)
        {
            _products.DeleteOne(Product => Product.Id == id);
        }

        // Updates a product in the database by replacing it with a new product object
        public void UpdateProductById(string id, Product product)
        {
            _products.ReplaceOne(Product => Product.Id == id, product);
        }

        /// Updates the status (e.g., "Active", "Inactive") of a product by its ID
        public void UpdateProductStatusById(string id, string status)
        {
            var update = Builders<Product>.Update.Set(p => p.Status, status);
            _products.UpdateOne(p => p.Id == id, update);
        }

        // Method to Activate/Deactivate products by Vendor ID and Category
        public void UpdateProductsStatusByVendorAndCategory(string vendorId, string category, string status)
        {
            // Create a filter to find products matching the given vendor ID and category
            var filter = Builders<Product>.Filter.Eq(p => p.VendorId, vendorId) & Builders<Product>.Filter.Eq(p => p.Category, category);
            // Create an update definition to set the new status
            var update = Builders<Product>.Update.Set(p => p.Status, status);
            // Update all matching products with the new status
            _products.UpdateMany(filter, update);
        }
    }
}
