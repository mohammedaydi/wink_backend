using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using wink.Models;

namespace wink.Services
{
    public class ItemService
    {
        private readonly IMongoCollection<Item> _itemsCollection;
        public ItemService(
          IOptions<WinkDatabaseSettings> winkDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                winkDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                winkDatabaseSettings.Value.DatabaseName);

            _itemsCollection = mongoDatabase.GetCollection<Item>(
                winkDatabaseSettings.Value.ItemsCollectionName);
        }


        public async Task<List<Item>> GetAsync() =>
       await _itemsCollection.Find(_ => true).ToListAsync();

        public async Task<Item?> GetAsync(string id) =>
            await _itemsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        public async Task<List<Item>> GetByCategoryAsync(string category) =>
        await _itemsCollection.Find(x => x.Category == category).ToListAsync();

        public async Task CreateAsync(Item newItem) =>
            await _itemsCollection.InsertOneAsync(newItem);

        public async Task UpdateAsync(string id, Item updatedItem) =>
            await _itemsCollection.ReplaceOneAsync(x => x.Id == id, updatedItem);

        public async Task RemoveAsync(string id) =>
            await _itemsCollection.DeleteOneAsync(x => x.Id == id);

        //new tasks
        public async Task<Item?> DecrementAsync(string id, int quantity)
        {
            var update = Builders<Item>.Update.Set("Quantity", quantity);
            var updatedDocument = await _itemsCollection.FindOneAndUpdateAsync((x) => x.Id == id,update, new FindOneAndUpdateOptions<Item>
            {
                ReturnDocument = ReturnDocument.After // Return the document after update
            });

            return updatedDocument;
        }

    }
}
