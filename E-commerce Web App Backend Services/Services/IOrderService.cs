using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(PlaceOrderDto placeOrderDto);
    }
}
