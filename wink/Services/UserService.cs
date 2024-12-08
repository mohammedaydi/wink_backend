using wink.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace wink.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UserService(
            IOptions<WinkDatabaseSettings> winkDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                winkDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                winkDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                winkDatabaseSettings.Value.UsersCollectionName);
        }

        //Tasks
        public async Task<List<User>> GetAsync() =>
       await _usersCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetAsync(string id) =>
            await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(User newBook) =>
            await _usersCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, User updatedBook) =>
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _usersCollection.DeleteOneAsync(x => x.Id == id);

    }
}
