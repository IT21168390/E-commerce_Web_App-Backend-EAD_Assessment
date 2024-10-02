﻿using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using E_commerce_Web_App_Backend_Services.Dto;

namespace E_commerce_Web_App_Backend_Services.services
{
    public interface IVendorRatingService
    {
        Task<IEnumerable<VendorRating>> GetAllRatings();
        Task<VendorRating> GetRatingById(string id);
        Task<VendorRating> AddRating(VendorRatingDTO ratingDTO);
        Task<VendorRating> UpdateRating(string id, VendorRatingDTO ratingDTO);
        Task<bool> DeleteRating(string id);
    }
}
