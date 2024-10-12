using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.ServicesImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace E_commerce_Web_App_Backend_Services.Controllers
{

    /// <summary>
    /// Handles user-related operations, including retrieval, creation, updating, and deletion of users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service used to manage user data.</param>
        public UserController(IUserService userService) 
        {
            this.userService = userService;
        }


        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>A list of users.</returns>
        // GET: api/<UserController>
        [HttpGet]
        public ActionResult<List<User>> Get()
        {
            return userService.Get();
        }

        // GET api/<UserController>/{id}
        [HttpGet("{id}")]
        public ActionResult<User> Get(string id)
        {
            var user = userService.Get(id);

            if (user == null)
            {
                return NotFound($"User with ID = {id} not found");
            }

            return user;
        }


        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">The user object to create.</param>
        /// <returns>The created user along with a 201 Created response.</returns>
        // POST api/<UserController>
        [HttpPost]
        public ActionResult<User> Post([FromBody] User user)
        {
            userService.Create(user);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // PUT api/<UserController>/{id}
        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] User user)
        {
            var existingUser = userService.Get(id);

            if (existingUser == null)
            {
                return NotFound($"User with ID = {id} not found");
            }

            userService.Update(id, user);

            return NoContent();
        }

        // DELETE api/<UserController>/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            var user = userService.Get(id);

            if (user == null)
            {
                return NotFound($"User with ID = {id} not found");
            }

            userService.Remove(user.Id);

            return Ok($"User with ID = {id} deleted");
        }


        /// <summary>
        /// Changes the status of a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user whose status is to be changed.</param>
        /// <param name="status">The new status to set for the user.</param>
        /// <returns>A message indicating the result of the status change.</returns>
        [HttpPut("activeUser/{id}")]
        [Authorize(Roles = "Administrator")]  // Only Admins can active users
        public IActionResult ChangeUserStatus(string id, [FromQuery] string status)
        {
            var user = userService.Get(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            userService.ChangeStatus(id, status);
            return Ok(new { message = "User status updated successfully" });
        }

    }
}
