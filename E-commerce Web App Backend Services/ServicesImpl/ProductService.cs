﻿using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<Inventory> _inventory;
        public ProductService(IDatabaseSettings settings, IMongoClient mongoClient) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.ProductCollectionName);
            _inventory = database.GetCollection<Inventory>(settings.InventoryCollectionName);
        }

        // Method to add a product
        public Product AddProduct(ProductDTO newProduct)
        {
            Product product = new Product {
                ProductId = newProduct.ProductId,
                Name = newProduct.Name, 
                Category = newProduct.Category, 
                VendorId = newProduct.VendorId, 
                Description = newProduct.Description, 
                Price = newProduct.Price, 
                Status = newProduct.Status, 
                CreatedAt = newProduct.CreatedAt };

            _products.InsertOne(product);
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


        // Method to get all product list
        public List<Product> GetAllProductList()
        {
            return _products.Find(Product => true).ToList();
        }

        // Method to get a product by id

        public Product GetProductById(string id)
        {
            return _products.Find(Product => Product.Id == id).FirstOrDefault();
        }


        // Method to remove a product
        public void RemoveProductById(string id)
        {
            _products.DeleteOne(Product => Product.Id == id);
        }


        // Method to update a product
        public void UpdateProductById(string id, Product product)
        {
            _products.ReplaceOne(Product => Product.Id == id, product);
        }

        // Method to Activate/Deactivate a product by Product ID
        public void UpdateProductStatusById(string id, string status)
        {
            var update = Builders<Product>.Update.Set(p => p.Status, status);
            _products.UpdateOne(p => p.Id == id, update);
        }

        // Method to Activate/Deactivate products by Vendor ID and Category
        public void UpdateProductsStatusByVendorAndCategory(string vendorId, string category, string status)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.VendorId, vendorId) & Builders<Product>.Filter.Eq(p => p.Category, category);
            var update = Builders<Product>.Update.Set(p => p.Status, status);
            _products.UpdateMany(filter, update);
        }
    }
}
