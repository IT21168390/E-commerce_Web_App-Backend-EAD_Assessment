using BCrypt.Net;
using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(IDatabaseSettings settings, IMongoClient mongoClient, IConfiguration config)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
            _jwtSecret = config["JwtSettings:Secret"];
            _jwtIssuer = config["JwtSettings:Issuer"];
            _jwtAudience = config["JwtSettings:Audience"];
        }

        // Method to authenticate a user
        public string Authenticate(UserLoginDTO userLoginDTO)
        {
            // Find the user by email
            var user = _users.Find(u => u.Email == userLoginDTO.Email).FirstOrDefault();

            // Check if the user exists and the password matches the hashed password
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLoginDTO.Password, user.Password) || user.Status != "Active")
            {
                // If user is null, password doesn't match, or user status is not active, return null
                return null;
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.Role, user.UserType)
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Method to register a user
        public User Register(UserRegisterDTO userRegisterDTO)
        {
            var user = new User
            {
                Name = userRegisterDTO.Name,
                Email = userRegisterDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userRegisterDTO.Password),  // Hashing the password
                UserType = userRegisterDTO.UserType,
                //Address = new Address
                //{
                //    Street = userRegisterDTO.Address.Street,
                //    City = userRegisterDTO.Address.City,
                //    ZipCode = userRegisterDTO.Address.ZipCode
                //},
                AccountCreatedAt = DateTime.UtcNow
            };

            // Set default status for customers and vendors
            if (user.UserType == "Customer" || user.UserType == "Vendor")
            {
                user.Status = "Inactive";
            }
            else
            {
                user.Status = "Active"; // Other user types can be active by default
            }

            _users.InsertOne(user);
            return user;
        }
    }
}
