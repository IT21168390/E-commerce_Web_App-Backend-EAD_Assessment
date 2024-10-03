using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;
using E_commerce_Web_App_Backend_Services.constants;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly INotificationService notificationService;
        public UserService(IDatabaseSettings settings, IMongoClient mongoClient, INotificationService notificationService) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
            this.notificationService = notificationService;
        }

        public User Create(User user)
        {
            _users.InsertOne(user);
            //***notification***//
            if(user.UserType == "Customer")
            {
               if (notificationService == null)
               {
                   throw new InvalidOperationException("Notification service is not initialized.");
               }
               object value = notificationService.CreateNotification(new Notification
               {
                   UserId = Constant.VendorId,
                   Message = "New customer account is registered, please review!",
               });
            }
            return user;
        }

        public List<User> Get()
        {
            return _users.Find(User => true).ToList();
        }

        public User Get(string id)
        {
            return _users.Find(User => User.Id == id).FirstOrDefault();
        }

        public void Remove(string id)
        {
            _users.DeleteOne(User => User.Id == id);
        }

        public void Update(string id, User user)
        {
            _users.ReplaceOne(User => User.Id == id, user);
        }
    }
}
