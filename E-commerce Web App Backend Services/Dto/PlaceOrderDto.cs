using System;
using System.Collections.Generic;

namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class PlaceOrderDto
    {
        public string CustomerId { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public AddressDto ShippingAddress { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }
}
