using Microsoft.AspNetCore.Mvc;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.Dto;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    /// <summary>
    /// Controller for managing inventory operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryController"/> class.
        /// </summary>
        /// <param name="inventoryService">The inventory service.</param>
        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        // POST: api/inventory
        /// <summary>
        /// Creates a new inventory item.
        /// </summary>
        /// <param name="inventory">The inventory item to create.</param>
        /// <returns>The created inventory item.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateInventory([FromBody] Inventory inventory)
        {
            await _inventoryService.CreateInventory(inventory);
            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Id }, inventory);
        }
        // GET: api/inventory
        /// <summary>
        /// Gets all inventory items.
        /// </summary>
        /// <returns>A list of all inventory items.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetAllInventory()
        {
            var inventories = await _inventoryService.GetAllInventory();
            return Ok(inventories);
        }

        // GET: api/inventory/{id}
        /// <summary>
        /// Gets a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item.</param>
        /// <returns>The inventory item with the specified ID.</returns>
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
        /// <summary>
        /// Updates a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to update.</param>
        /// <param name="updatedInventory">The updated inventory data.</param>
        /// <returns>No content if the update is successful.</returns>
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
        /// <summary>
        /// Updates the inventory item for a purchase by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to update.</param>
        /// <param name="updatedInventory">The updated inventory data for the purchase.</param>
        /// <returns>No content if the update is successful.</returns>
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
        /// <summary>
        /// Deletes a specific inventory item by ID.
        /// </summary>
        /// <param name="id">The ID of the inventory item to delete.</param>
        /// <returns>No content if the deletion is successful, or a bad request if there are pending orders.</returns>
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