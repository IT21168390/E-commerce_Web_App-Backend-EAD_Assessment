using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IProductService
    {
        List<ProductDTO> GetAllProductList();
        Product GetProductById(string id);
        Product AddProduct(ProductDTO product);
        void UpdateProductById(string id, Product product);
        void RemoveProductById(string id);

        void UpdateProductStatusById(string id, string status);
        void UpdateProductsStatusByVendorAndCategory(string vendorId, string category, string status);
    }
}
