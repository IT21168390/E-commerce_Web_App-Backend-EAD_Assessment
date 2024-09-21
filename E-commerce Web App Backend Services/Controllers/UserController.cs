using E_commerce_Web_App_Backend_Services.Services;
using E_commerce_Web_App_Backend_Services.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public UserController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet]
        public async Task<List<User>> Get() {
            return await _mongoDBService.GetUserAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Post(/*[FromBody] User user*/) {
            User user = new User { Name = "ABC", Email = "abc@email.com", UserType = "Test"};
            await _mongoDBService.CreateUserAsync(user);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }
        /*// GET: UserController
        public ActionResult Index()
        {
            return View();
        }

        // GET: UserController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }*/
    }
}
