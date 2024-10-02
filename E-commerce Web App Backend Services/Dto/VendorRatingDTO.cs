namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class VendorRatingDTO
    {
        public string CustomerId { get; set; }
        public string VendorId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
    }
}
