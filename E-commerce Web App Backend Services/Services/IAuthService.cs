using E_commerce_Web_App_Backend_Services.Dto;

namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IAuthService
    {
        AuthResponseDTO Authenticate(UserLoginDTO userLoginDTO); // For login
        User Register(UserRegisterDTO userRegisterDTO);  // For registering new users
    }
}
