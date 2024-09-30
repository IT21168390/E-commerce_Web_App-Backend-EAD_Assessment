using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(string orderId);
        Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId, int pageNumber, int pageSize);
        Task<List<Order>> GetAllOrdersAsync(int pageNumber, int pageSize);

        Task<Order> PlaceOrderAsync(PlaceOrderDto placeOrderDto);
        Task<Order> UpdateOrderAsync(string orderId, UpdateOrderDto updateOrderDto);
        Task<Order> UpdateVendorOrderStatusAsync(string orderId, string vendorId, string status);
        //Task<Order> CancelOrderAsync(string orderId);
        Task<Order> RequestCancelOrderAsync(string orderId);
        Task<Order> ConfirmCancelOrderAsync(string orderId);
    }
}
