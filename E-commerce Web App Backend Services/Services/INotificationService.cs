using E_commerce_Web_App_Backend_Services.models;
namespace E_commerce_Web_App_Backend_Services.Services
{
        public interface INotificationService
        {
            Task<Notification> CreateNotification(Notification notification);
            Task<Notification> GetNotificationById(string notificationId);
            Task<IEnumerable<Notification>> GetAllNotificationsByUserId(string userId);
            Task<bool> MarkAsRead(string notificationId);
            Task<bool> DeleteNotification(string notificationId);
        }
}