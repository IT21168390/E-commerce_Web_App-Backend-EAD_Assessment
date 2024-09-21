using E_commerce_Web_App_Backend_Services.models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;

namespace E_commerce_Web_App_Backend_Services.Services;

public class MongoDBService
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<User> _productCollection;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _userCollection = database.GetCollection<User>(mongoDBSettings.Value.UserCollectionName);
        _productCollection = database.GetCollection<User>(mongoDBSettings.Value.ProductCollectionName);
    }

    public async Task CreateUserAsync(User user)
    {
        await _userCollection.InsertOneAsync(user);
        return;
    }

    public async Task<List<User>> GetUserAsync()
    {
        return await _userCollection.Find(new BsonDocument()).ToListAsync();
    }
}