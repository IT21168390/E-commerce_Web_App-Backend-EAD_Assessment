using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.ServicesImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        // GET: api/<ProductController>
        [HttpGet("GetAllProductList")]
        public ActionResult<List<ProductDTO>> Get()
        {
            return productService.GetAllProductList();
        }

        // GET api/<ProductController>/5
        [HttpGet("GetProductById/{id}")]
        public ActionResult<Product> Get(string id)
        {
            var product = productService.GetProductById(id);

            if (product == null)
            {
                return NotFound($"Product with ID = {id} not found");
            }

            return product;
        }

        // POST api/<ProductController>
        [HttpPost("AddProduct")]
       //[Authorize(Roles = "Vendor")]
        public ActionResult<Product> Post([FromBody] ProductDTO product)
        {
            productService.AddProduct(product);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        // PUT api/<ProductController>/5
        [HttpPut("UpdateProductById/{id}")]
        [Authorize(Roles = "Vendor")]
        public ActionResult Put(string id, [FromBody] Product product)
        {
            var existingProduct = productService.GetProductById(id);

            if (existingProduct == null)
            {
                return NotFound($"Product with ID = {id} not found");
            }

            productService.UpdateProductById(id, product);

            return NoContent();
        }

        // DELETE api/<ProductController>/5
        [HttpDelete("RemoveProductById/{id}")]
        [Authorize(Roles = "Vendor")]
        public ActionResult Delete(string id)
        {
            var product = productService.GetProductById(id);

            if (product == null)
            {
                return NotFound($"Product with ID = {id} not found");
            }

            productService.RemoveProductById(product.Id);

            return Ok($"Product with ID = {id} deleted");
        }

        // Activate/Deactivate a product by Product ID
        [HttpPut("UpdateProductStatusById/{id}")]
        [Authorize(Roles = "Administrator")]
        public ActionResult UpdateProductStatusById(string id, [FromQuery] string status)
        {
            var existingProduct = productService.GetProductById(id);
            if (existingProduct == null)
            {
                return NotFound($"Product with ID = {id} not found");
            }

            productService.UpdateProductStatusById(id, status);
            return Ok($"Product with ID = {id} status updated to {status}");
        }

        // Activate/Deactivate products by Vendor ID and Category
        [HttpPut("UpdateProductsStatusByVendorAndCategory")]
        [Authorize(Roles = "Administrator")]
        public ActionResult UpdateProductsStatusByVendorAndCategory([FromQuery] string vendorId, [FromQuery] string category, [FromQuery] string status)
        {
            productService.UpdateProductsStatusByVendorAndCategory(vendorId, category, status);
            return Ok($"Products under Vendor ID = {vendorId} and Category = {category} updated to status = {status}");
        }
    }
}
