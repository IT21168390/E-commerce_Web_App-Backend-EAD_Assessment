namespace E_commerce_Web_App_Backend_Services.Dto
{
    public class UserRegisterDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }  // Administrator, Vendor, CSR
    }
}
