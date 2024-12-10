using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using wink.Models;

namespace wink.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public AuthService(
            IOptions<WinkDatabaseSettings> winkDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                winkDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                winkDatabaseSettings.Value.DatabaseName);

            _usersCollection = mongoDatabase.GetCollection<User>(
                winkDatabaseSettings.Value.UsersCollectionName);
        }

        public async Task<User> getUsersCrededintials(string email)
        {
            return await _usersCollection.Find((x) => x.Email == email).FirstOrDefaultAsync();
        }


     
    }
}
