using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace E_commerce_Web_App_Backend_Services.ServicesImpl
{
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationService(IOptions<DatabaseSettings> dbSettings)
        {
            var client = new MongoClient(dbSettings.Value.ConnectionString);
            var database = client.GetDatabase(dbSettings.Value.DatabaseName);
            _notifications = database.GetCollection<Notification>(dbSettings.Value.NotificationCollectionName);
        }

        public async Task<IEnumerable<Notification>> GetAllNotificationsByUserId(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
            return await _notifications.Find(filter).ToListAsync();
        }

        public async Task<Notification> GetNotificationById(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            return await _notifications.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Notification> CreateNotification(Notification notification)
        {
            notification.CreatedAt = DateTime.Now;
            notification.IsRead = false;
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        public async Task<bool> MarkAsRead(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            var result = await _notifications.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNotification(string notificationId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var result = await _notifications.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}