//namespace E_commerce_Web_App_Backend_Services.Controllers
//{
//    public class VendorRating
//    {
//    }
//}

using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorRatingController : ControllerBase
    {
        private readonly IVendorRatingService _vendorRatingService;

        public VendorRatingController(IVendorRatingService vendorRatingService)
        {
            _vendorRatingService = vendorRatingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorRating>>> GetAllRatings()
        {
            var ratings = await _vendorRatingService.GetAllRatings();
            return Ok(ratings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VendorRating>> GetRatingById(string id)
        {
            var rating = await _vendorRatingService.GetRatingById(id);
            if (rating == null) return NotFound();
            return Ok(rating);
        }

        [HttpPost]
        public async Task<ActionResult<VendorRating>> AddRating([FromBody] VendorRatingDTO ratingDTO)
        {
            var newRating = await _vendorRatingService.AddRating(ratingDTO);
            return CreatedAtAction(nameof(GetRatingById), new { id = newRating.Id }, newRating);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VendorRating>> UpdateRating(string id, [FromBody] VendorRatingDTO ratingDTO)
        {
            var updatedRating = await _vendorRatingService.UpdateRating(id, ratingDTO);
            if (updatedRating == null) return NotFound();
            return Ok(updatedRating);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRating(string id)
        {
            var deleted = await _vendorRatingService.DeleteRating(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
