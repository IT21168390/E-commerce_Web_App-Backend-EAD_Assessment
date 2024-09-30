using Microsoft.AspNetCore.Mvc;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;


namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }


        [HttpPost]
        public async Task<ActionResult<Notification>> CreateNotification([FromBody] Notification notification)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdNotification = await _notificationService.CreateNotification(notification);
            return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.Id }, createdNotification);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotificationById(string id)
        {
            var notification = await _notificationService.GetNotificationById(id);
            if (notification == null)
                return NotFound();

            return Ok(notification);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetAllNotificationsByUserId(string userId)
        {
            var notifications = await _notificationService.GetAllNotificationsByUserId(userId);
            if (notifications == null)
                return NotFound();

            return Ok(notifications);
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<ActionResult> MarkAsRead(string id)
        {
            var result = await _notificationService.MarkAsRead(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

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
