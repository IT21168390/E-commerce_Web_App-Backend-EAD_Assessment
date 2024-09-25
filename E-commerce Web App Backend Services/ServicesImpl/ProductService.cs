using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;
        public ProductService(IDatabaseSettings settings, IMongoClient mongoClient) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _products = database.GetCollection<Product>(settings.ProductCollectionName);
        }
        public Product AddProduct(Product product)
        {
            _products.InsertOne(product);
            return product;
        }

        public List<Product> GetAllProductList()
        {
            return _products.Find(Product => true).ToList();
        }

        public Product GetProductById(string id)
        {
            return _products.Find(Product => Product.Id == id).FirstOrDefault();
        }

        public void RemoveProductById(string id)
        {
            _products.DeleteOne(Product => Product.Id == id);
        }

        public void UpdateProductById(string id, Product product)
        {
            _products.ReplaceOne(Product => Product.Id == id, product);
        }
    }
}
