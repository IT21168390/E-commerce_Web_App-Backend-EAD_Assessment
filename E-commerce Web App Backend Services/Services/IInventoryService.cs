using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetAllInventory();
        Task<Inventory> GetInventoryById(string id);
        Task CreateInventory(Inventory inventory);
        Task<Inventory> UpdateInventory(string id, InventoryUpdateDto updatedInventory);
        Task<Inventory> UpdateInventoryBuy(string id, InventoryDto updatedInventory);
        Task<bool> DeleteInventory(string id);
        Task<bool> CheckLowStockAndNotify(string vendorId);
        Task<Inventory> GetInventoryByProductIdAsync(string productId);
        Task UpdateInventoryAsync(Inventory inventory);
    }
}