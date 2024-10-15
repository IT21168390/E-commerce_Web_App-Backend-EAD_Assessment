using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly INotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="settings">The database settings containing connection information.</param>
        /// <param name="mongoClient">The MongoDB client used to connect to the database.</param>
        /// <param name="notificationService">The service responsible for handling notifications.</param>
        public UserService(IDatabaseSettings settings, IMongoClient mongoClient, INotificationService notificationService) 
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
            this.notificationService = notificationService;
        }


        /// <summary>
        /// Creates a new user in the database and triggers notifications if the user is a customer.
        /// </summary>
        /// <param name="user">The user object to be created.</param>
        /// <returns>The newly created user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the notification service is not initialized when required.</exception>
        public async Task<User> Create(User user)
        {
            // Set default status for customers and vendors
            if (user.UserType == Constant.CUSTOMER || user.UserType == Constant.VENDOR)
            {
                user.Status = Constant.INACTIVE;
            }
            else
            {
                user.Status = Constant.ACTIVE; // Other user types can be active by default
            }

            _users.InsertOne(user);
            //***notification***//
            if(user.UserType == Constant.CUSTOMER)
            {
               if (notificationService == null)
               {
                   throw new InvalidOperationException("Notification service is not initialized.");
               }

               //fetch users whose userType is csr from user collection
               var csrUsers = await _users.Find(u => u.UserType == Constant.CSR).ToListAsync();
                foreach (var csrUser in csrUsers)
                {
                     object value = notificationService.CreateNotification(new Notification
                     {
                          UserId = csrUser.Id,
                          Message = "New customer account has been registered, please review!",
                     });
                }
            }
            return user;
        }



        /// <summary>
        /// Updates the status of a user in the database.
        /// </summary>
        /// <param name="id">The ID of the user whose status is to be updated.</param>
        /// <param name="status">The new status to be set for the user.</param>
        public void ChangeStatus(string id, string status)
        {
            var update = Builders<User>.Update.Set(u => u.Status, status);
            _users.UpdateOne(User => User.Id == id, update);
        }



        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all users.</returns>
        public List<User> Get()
        {
            return _users.Find(User => true).ToList();
        }



        /// <summary>
        /// Retrieves a user by their ID from the database.
        /// </summary>
        /// <param name="id">The ID of the user to be retrieved.</param>
        /// <returns>The user with the specified ID, or null if not found.</returns>
        public User Get(string id)
        {
            return _users.Find(User => User.Id == id).FirstOrDefault();
        }



        /// <summary>
        /// Deletes a user by their ID from the database.
        /// </summary>
        /// <param name="id">The ID of the user to be deleted.</param>
        public void Remove(string id)
        {
            _users.DeleteOne(User => User.Id == id);
        }



        /// <summary>
        /// Updates an existing user's details in the database.
        /// </summary>
        /// <param name="id">The ID of the user to be updated.</param>
        /// <param name="user">The updated user object.</param>
        public void Update(string id, User user)
        {
            var existingUser = _users.Find(u => u.Id == id).FirstOrDefault();
            if (existingUser != null)
            {
                existingUser.Name = user.Name ?? existingUser.Name;
                existingUser.Email = user.Email ?? existingUser.Email;
                existingUser.Password = user.Password ?? existingUser.Password;
                existingUser.UserType = user.UserType ?? existingUser.UserType;
                existingUser.Status = user.Status ?? existingUser.Status;
                existingUser.Address = user.Address ?? existingUser.Address;
  
                _users.ReplaceOne(u => u.Id == id, existingUser);
            }
        }

        public void UpdateUserById(UserDTO user)
        {
            var existingUser = _users.Find(u => u.Id == user.Id).FirstOrDefault();


            existingUser.Name = user.Name;
            _users.ReplaceOne(u => u.Id == user.Id, existingUser);
        }

    }
}
