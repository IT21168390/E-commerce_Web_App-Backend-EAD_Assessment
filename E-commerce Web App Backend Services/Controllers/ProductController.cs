using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.ServicesImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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


        // GET: api/<ProductController>/GetAllProductList
        /// <summary>
        /// Retrieves a list of all products.
        /// </summary>
        /// <returns>List of ProductDTO objects containing product information.</returns>
        [HttpGet("GetAllProductList")]
        public ActionResult<List<ProductDTO>> Get()
        {
            return productService.GetAllProductList();
        }



        // GET api/<ProductController>/GetProductById/{id}
        /// <summary>
        /// Retrieves a product by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the product.</param>
        /// <returns>A Product object if found, otherwise a NotFound result.</returns>
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



        // POST api/<ProductController>/AddProduct
        /// <summary>
        /// Adds a new product to the product list.
        /// </summary>
        /// <param name="product">The ProductDTO object containing the details of the product to be added.</param>
        /// <returns>The created Product object with status 201 Created.</returns>
        [HttpPost("AddProduct")]
       [Authorize(Roles = "Vendor")]
        public ActionResult<Product> Post([FromBody] ProductDTO product)
        {
            productService.AddProduct(product);

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }



        // PUT api/<ProductController>/UpdateProductById/{id}
        /// <summary>
        /// Updates an existing product by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the product to be updated.</param>
        /// <param name="product">The updated Product object.</param>
        /// <returns>No content if the update is successful, otherwise NotFound result if the product does not exist.</returns>
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


        // DELETE api/<ProductController>/RemoveProductById/{id}
        /// <summary>
        /// Removes a product by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the product to be removed.</param>
        /// <returns>Ok result if the deletion is successful, otherwise NotFound result if the product does not exist.</returns>
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



        // PUT api/<ProductController>/UpdateProductStatusById/{id}
        /// <summary>
        /// Activates or deactivates a product by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the product to update.</param>
        /// <param name="status">The new status for the product ("Active" or "Inactive").</param>
        /// <returns>Ok result if the status update is successful, otherwise NotFound result if the product does not exist.</returns>
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



        // PUT api/<ProductController>/UpdateProductsStatusByVendorAndCategory
        /// <summary>
        /// Activates or deactivates products by vendor ID and category.
        /// </summary>
        /// <param name="vendorId">The ID of the vendor whose products are to be updated.</param>
        /// <param name="category">The category of products to be updated.</param>
        /// <param name="status">The new status for the products ("Active" or "Inactive").</param>
        /// <returns>Ok result indicating the status update was successful for the specified products.</returns>
        [HttpPut("UpdateProductsStatusByVendorAndCategory")]
        [Authorize(Roles = "Administrator")]
        public ActionResult UpdateProductsStatusByVendorAndCategory([FromQuery] string vendorId, [FromQuery] string category, [FromQuery] string status)
        {
            productService.UpdateProductsStatusByVendorAndCategory(vendorId, category, status);
            return Ok($"Products under Vendor ID = {vendorId} and Category = {category} updated to status = {status}");
        }
    }
}
