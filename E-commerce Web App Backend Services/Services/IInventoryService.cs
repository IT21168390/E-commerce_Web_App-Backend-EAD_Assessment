using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IInventoryService
    {
        Task<Inventory> GetInventoryByProductIdAsync(string productId);
        Task UpdateInventoryAsync(Inventory inventory);
    }
}
