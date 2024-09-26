using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        public UserService(IDatabaseSettings settings, IMongoClient mongoClient) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
        }

        public User Create(User user)
        {
            _users.InsertOne(user);
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
