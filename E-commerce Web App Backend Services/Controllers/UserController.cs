using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.ServicesImpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService) 
        {
            this.userService = userService;
        }

        // GET: api/<UserController>
        [HttpGet]
        public ActionResult<List<User>> Get()
        {
            return userService.Get();
        }

        // GET api/<UserController>/5
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

        // POST api/<UserController>
        [HttpPost]
        public ActionResult<User> Post([FromBody] User user)
        {
            userService.Create(user);

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        // PUT api/<UserController>/5
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

        // DELETE api/<UserController>/5
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
