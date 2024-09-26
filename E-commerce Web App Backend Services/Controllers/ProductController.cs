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
        public ActionResult<List<Product>> Get()
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
        [Authorize(Roles = "Vendor")]
        public ActionResult<Product> Post([FromBody] Product product)
        {
            productService.AddProduct(product);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        // PUT api/<ProductController>/5
        [HttpPut("UpdateProductById/{id}")]
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
    }
}
