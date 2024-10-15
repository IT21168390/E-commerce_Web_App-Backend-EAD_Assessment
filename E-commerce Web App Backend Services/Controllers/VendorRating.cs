
using DnsClient;
using E_commerce_Web_App_Backend_Services.Dto;
using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Numerics;

namespace E_commerce_Web_App_Backend_Services.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorRatingController : ControllerBase
    {
        private readonly IVendorRatingService _vendorRatingService;

        /// <summary> 
        /// Initializes a new instance of the VendorRatingController with the specified vendor rating service.
        /// </summary> 
        /// <param name="vendorRatingService">The service used for managing vendor ratings.</param> 

        public VendorRatingController(IVendorRatingService vendorRatingService)
        {
            _vendorRatingService = vendorRatingService;
        }

        /// <summary> 
        /// Retrieves all vendor ratings. 
        /// </summary> 
        /// <returns>A list of all vendor ratings.</returns> 

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorRating>>> GetAllRatings()
        {
            var ratings = await _vendorRatingService.GetAllRatings();
            return Ok(ratings);
        }

        /// <summary> 
        /// Retrieves a vendor rating by its ID. 
        /// </summary> 
        /// <param name="id">The ID of the vendor rating to retrieve.</param> 
        /// <returns>The vendor rating with the specified ID, or a 404 error if not found.</returns> 


                [HttpGet("{id}")]
        public async Task<ActionResult<VendorRating>> GetRatingById(string id)
        {
            var rating = await _vendorRatingService.GetRatingById(id);
            if (rating == null) return NotFound();
            return Ok(rating);
        }

        /// <summary> 
        /// Adds a new vendor rating. 
        /// </summary> 
        /// <param name="ratingDTO">The data transfer object containing vendor rating information.</param> 
        /// <returns>The created vendor rating and its location URI.</returns> 

                [HttpPost]
        public async Task<ActionResult<VendorRating>> AddRating([FromBody] VendorRatingDTO ratingDTO)
        {
            var newRating = await _vendorRatingService.AddRating(ratingDTO);
            return CreatedAtAction(nameof(GetRatingById), new { id = newRating.Id }, newRating);
        }

        /// <summary> 
        /// Updates an existing vendor rating. 
        /// </summary> 
        /// <param name="id">The ID of the vendor rating to update.</param> 
        /// <param name="ratingDTO">The updated data transfer object containing vendor rating information.</param> 
        /// <returns>The updated vendor rating, or a 404 error if not found.</returns>

        [HttpPut("{id}")]
        public async Task<ActionResult<VendorRating>> UpdateRating(string id, [FromBody] VendorRatingDTO ratingDTO)
        {
            var updatedRating = await _vendorRatingService.UpdateRating(id, ratingDTO);
            if (updatedRating == null) return NotFound();
            return Ok(updatedRating);
        }

        /// <summary> 
        /// Deletes a vendor rating by its ID. 
        /// </summary> 
        /// <param name="id">The ID of the vendor rating to delete.</param> 
        /// <returns>No content response if deletion is successful, or a 404 error if not found.</returns> 


                [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRating(string id)
        {
            var deleted = await _vendorRatingService.DeleteRating(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        // New API to get ratings by CustomerId
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<VendorRating>>> GetRatingsByCustomerId(string customerId)
        {
            var ratings = await _vendorRatingService.GetRatingsByCustomerId(customerId);
            if (ratings == null || !ratings.Any()) return NotFound();
            return Ok(ratings);
        }
    }
}
