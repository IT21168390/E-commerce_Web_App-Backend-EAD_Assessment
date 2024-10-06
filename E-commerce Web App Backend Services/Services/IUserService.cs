namespace E_commerce_Web_App_Backend_Services.Services
{
    public interface IUserService
    {
        List<User> Get();
        User Get(string id);
        Task<User> Create(User user);
        void Update(string id, User user);
        void Remove(string id);
        void ChangeStatus(string id, string status);
    }
}
