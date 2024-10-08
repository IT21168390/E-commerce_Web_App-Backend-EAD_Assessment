using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    /// <summary>
    /// Service for managing notification operations.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="dbSettings">The database settings.</param>
        public NotificationService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _notifications = database.GetCollection<Notification>(dbSettings.Value.NotificationCollectionName);
        }

        /// <summary>
        /// Gets all notifications for a specific user by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of notifications for the specified user.</returns>
        public async Task<IEnumerable<Notification>> GetAllNotificationsByUserId(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
            return await _notifications.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Gets a specific notification by ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification.</param>
        /// <returns>The notification with the specified ID.</returns>
        public async Task<Notification> GetNotificationById(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            return await _notifications.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="notification">The notification to create.</param>
        /// <returns>The created notification.</returns>
        public async Task<Notification> CreateNotification(Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        /// <summary>
        /// Marks a specific notification as read by ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        /// <returns>True if the operation is successful, otherwise false.</returns>
        public async Task<bool> MarkAsRead(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            var result = await _notifications.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Deletes a specific notification by ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete.</param>
        /// <returns>True if the deletion is successful, otherwise false.</returns>
        public async Task<bool> DeleteNotification(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var result = await _notifications.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}