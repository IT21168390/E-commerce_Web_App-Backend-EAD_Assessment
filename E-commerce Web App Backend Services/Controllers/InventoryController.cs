using Microsoft.AspNetCore.Mvc;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.Dto;


namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<ActionResult> CreateInventory([FromBody] Inventory inventory)
        {
            await _inventoryService.CreateInventory(inventory);
            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Id }, inventory);
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAllInventory()
        {
            var inventories = await _inventoryService.GetAllInventory();
            return Ok(inventories);
        }

        // GET: api/inventory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(string id)
        {
            var inventory = await _inventoryService.GetInventoryById(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return Ok(inventory);
        }

                // PUT: api/inventory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(string id, [FromBody] InventoryUpdateDto updatedInventory)
        {
            var inventory = await _inventoryService.UpdateInventory(id, updatedInventory);
            if (inventory == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // PUT: api/inventory/{id}
        [HttpPut("buy/{id}")]
        public async Task<IActionResult> UpdateInventoryBuy(string id, [FromBody] InventoryDto updatedInventory)
        {
            var inventory = await _inventoryService.UpdateInventoryBuy(id, updatedInventory);
            if (inventory == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/inventory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(string id)
        {
            var result = await _inventoryService.DeleteInventory(id);
            if (!result)
            {
                return BadRequest("Cannot delete inventory that has pending orders.");
            }
            return NoContent();
        }
    }
}
