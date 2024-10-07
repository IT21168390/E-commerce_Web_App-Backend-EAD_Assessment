using E_commerce_Web_App_Backend_Services.models;
using E_commerce_Web_App_Backend_Services.Dto;
using MongoDB.Driver;


namespace E_commerce_Web_App_Backend_Services.Services
{
    public class VendorRatingService : IVendorRatingService
    {
        private readonly IMongoCollection<VendorRating> _vendorRatings;

        // Constructor to initialize the MongoDB collection
        public VendorRatingService(IDatabaseSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _vendorRatings = database.GetCollection<VendorRating>(settings.VendorRatingCollectionName);
        }

        // Retrieve all vendor ratings
        public async Task<IEnumerable<VendorRating>> GetAllRatings()
        {
            return await _vendorRatings.Find(rating => true).ToListAsync();
        }

        // Retrieve a specific rating by ID
        public async Task<VendorRating> GetRatingById(string id)
        {
            return await _vendorRatings.Find(rating => rating.Id == id).FirstOrDefaultAsync();
        }

        // Add a new rating to the collection
        public async Task<VendorRating> AddRating(VendorRatingDTO ratingDTO)
        {
            var newRating = new VendorRating
            {
                CustomerId = ratingDTO.CustomerId,
                VendorId = ratingDTO.VendorId,
                OrderID = ratingDTO.OrderID,
                Rating = ratingDTO.Rating,
                Comment = ratingDTO.Comment,
                CreatedAt = DateTime.UtcNow
            };
            await _vendorRatings.InsertOneAsync(newRating);
            return newRating;
        }

        // Update an existing rating by its ID
        public async Task<VendorRating> UpdateRating(string id, VendorRatingDTO ratingDTO)
        {
            var updateDefinition = Builders<VendorRating>.Update
                .Set(r => r.Rating, ratingDTO.Rating)
                .Set(r => r.Comment, ratingDTO.Comment);

            var options = new FindOneAndUpdateOptions<VendorRating> { ReturnDocument = ReturnDocument.After };

            // Use FindOneAndUpdateAsync with clear type definitions to avoid ambiguity
            return await _vendorRatings.FindOneAndUpdateAsync<VendorRating, VendorRating>(
                rating => rating.Id == id,
                updateDefinition,
                options
            );

        }

        // Delete a rating by its ID
        public async Task<bool> DeleteRating(string id)
        {
            var result = await _vendorRatings.DeleteOneAsync(rating => rating.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
