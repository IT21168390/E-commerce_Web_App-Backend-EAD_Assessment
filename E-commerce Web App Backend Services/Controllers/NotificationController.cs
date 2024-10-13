using Microsoft.AspNetCore.Mvc;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    /// <summary>
    /// Controller for managing notification operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notificationService">The notification service.</param>
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Creates a new notification.
        /// </summary>
        /// <param name="notification">The notification to create.</param>
        /// <returns>The created notification.</returns>
        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification([FromBody] Notification notification)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdNotification = await _notificationService.CreateNotification(notification);
            return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.Id }, createdNotification);
        }

        /// <summary>
        /// Gets a specific notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification.</param>
        /// <returns>The notification with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotificationById(string id)
        {
            var notification = await _notificationService.GetNotificationById(id);
            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        /// <summary>
        /// Gets all notifications for a specific user by user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of notifications for the specified user.</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAllNotificationsByUserId(string userId)
        {
            var notifications = await _notificationService.GetAllNotificationsByUserId(userId);
            if (notifications == null)
                return NotFound();

            return Ok(notifications);
        }

        /// <summary>
        /// Marks a specific notification as read by ID.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <returns>No content if the operation is successful.</returns>
        [HttpPut("{id}/mark-as-read")]
        public async Task<ActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.MarkAsRead(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Get a specific unread notification count by userId.
        /// </summary>
        /// <param name="userId">The ID of the user that need to get notification count.</param>
        /// <returns>No of count in integer.</returns>
        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<int>> GetUnreadNotificationCount(string userId)
        {
            var count = await _notificationService.GetUnreadNotificationCount(userId);
            return Ok(count);
        }

        /// <summary>
        /// Deletes a specific notification by ID.
        /// </summary>
        /// <param name="id">The ID of the notification to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(string id)
        {
            var result = await _notificationService.DeleteNotification(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}