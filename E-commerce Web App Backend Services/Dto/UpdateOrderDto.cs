namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class UpdateOrderDto
    {
        public List<OrderItemDto>? OrderItems { get; set; }
        public AddressDto ShippingAddress { get; set; }
    }
}
