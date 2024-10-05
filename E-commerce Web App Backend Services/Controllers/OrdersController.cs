using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Place a new order.
        /// </summary>
        /// <param name="placeOrderDto">Order details.</param>
        /// <returns>Created order.</returns>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto placeOrderDto)
        {
            try
            {
                var order = await _orderService.PlaceOrderAsync(placeOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }



        /// <summary>
        /// Updates the status of a specific order to "Dispatched" if the current status is "Pending".
        /// Deducts stock from the inventory for each item in the order.
        /// </summary>
        /// <param name="orderId">The ID of the order to be dispatched.</param>
        /// <returns>The updated order.</returns>
        [HttpPatch("dispatch/{orderId}")]
        public async Task<IActionResult> DispatchOrderStatus(string orderId)
        {
            try
            {
                // Attempt to dispatch the order by updating its status to "Dispatched"
                var updatedOrder = await _orderService.DispatchOrderStatusAsync(orderId);

                // If successful, return the updated order details
                return Ok(updatedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                // Return 404 Not Found if the order was not found
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Return 400 Bad Request if there was an issue with the order status or invalid product ID
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Return 409 Conflict if there is insufficient stock for any product
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Return 500 Internal Server Error for any other exceptions
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }



        /// <summary>
        /// Get order by ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Order details.</returns>
        /*[HttpGet("{id:length(24)}", Name = "GetOrderById")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            // Implement this method if needed
            return Ok();
        }*/

        // GET: api/<OrdersController>
        /*[HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }*/

        // GET api/<OrdersController>/5
        /*[HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }*/

        // POST api/<OrdersController>
        /*[HttpPost]
        public void Post([FromBody] string value)
        {
        }*/


        /// <summary>
        /// Request to cancel an existing order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Order details with updated status.</returns>
        [HttpPatch("request-cancel/{id}")]
        public async Task<IActionResult> RequestCancelOrder(string id)
        {
            try
            {
                var order = await _orderService.RequestCancelOrderAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Confirm the cancellation of an existing order.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Order details with updated status.</returns>
        [HttpPatch("confirm-cancel/{id}")]
        public async Task<IActionResult> ConfirmCancelOrder(string id)
        {
            try
            {
                var order = await _orderService.ConfirmCancelOrderAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }
        // PUT api/<OrdersController>/5
        /*[HttpPatch("/cancel/{id}")]
        public async Task<IActionResult> CancelOrder(string id)
        {
            try
            {
                var cancelledOrder = await _orderService.CancelOrderAsync(id);
                return Ok(cancelledOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }*/

        // PUT api/<OrdersController>/5
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderDto updateOrderDto)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, updateOrderDto);
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }


        [HttpPatch("deliver/{orderId}")]
        public async Task<IActionResult> UpdateVendorOrderStatus(string orderId, [FromQuery] string vendorId, [FromQuery] string status)
        {
            try
            {
                // Call the service to update the vendor order status
                var updatedOrder = await _orderService.UpdateVendorOrderStatusAsync(orderId, vendorId, status);

                // Return the updated order
                return Ok(updatedOrder);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { message = "An error occurred while updating the order status.", details = ex.Message });
            }
        }





        /// <summary>
        /// Gets a specific order by its ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>The order details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Gets all orders with optional pagination.
        /// </summary>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Number of orders per page.</param>
        /// <returns>A list of orders.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Gets all orders by vendor ID with optional pagination.
        /// </summary>
        /// <param name="vendorId">Vendor ID.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Number of orders per page.</param>
        /// <returns>A list of orders associated with the vendor.</returns>
        [HttpGet("vendor/{vendorId}")]
        public async Task<IActionResult> GetOrdersByVendorId(string vendorId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetOrdersByVendorIdAsync(vendorId, pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }


        /// <summary>
        /// Gets all orders by customer ID with optional pagination.
        /// </summary>
        /// <param name="customerId">Customer ID.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <param name="pageSize">Number of orders per page.</param>
        /// <returns>A list of orders associated with the customer.</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerId(string customerId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId, pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
            }
        }




        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
