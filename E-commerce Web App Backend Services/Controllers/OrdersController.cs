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
        /// Get order by ID.
        /// </summary>
        /// <param name="id">Order ID.</param>
        /// <returns>Order details.</returns>
        [HttpGet("{id:length(24)}", Name = "GetOrderById")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            // Implement this method if needed
            return Ok();
        }

        // GET: api/<OrdersController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

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

        // PUT api/<OrdersController>/5
        [HttpPatch("/cancel/{id}")]
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
        }

        // PUT api/<OrdersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<OrdersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
