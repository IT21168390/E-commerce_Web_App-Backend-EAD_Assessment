using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IProductService
    {
        List<Product> GetAllProductList();
        Product GetProductById(string id);
        Product AddProduct(Product product);
        void UpdateProductById(string id, Product product);
        void RemoveProductById(string id);
    }
}
