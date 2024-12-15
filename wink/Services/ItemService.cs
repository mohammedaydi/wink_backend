using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

        public async Task<List<Item>> GetItemsByIdsAsync(List<string> ids)
        {
            var objectIdList = ids.Select(id => new ObjectId(id)).ToList();
            var projection = Builders<Item>.Projection
            .Exclude(c => c.Description)
            .Exclude(c => c.Quantity)
            .Exclude(c => c.Image);

            // Create a filter using the $in operator
            var filter = Builders<Item>.Filter.In("_id", objectIdList);

            // Query the collection
            var items = await _itemsCollection.Find(filter).ToListAsync();
            
            return items;
        }

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
            if(quantity < 0) return null;

            var item = _itemsCollection.Find(x => x.Id == id);
            if(item == null) return null;
            var update = Builders<Item>.Update.Set("Quantity", quantity);
            try
            {
                var updatedDocument = await _itemsCollection.FindOneAndUpdateAsync((x) => x.Id == id, update, new FindOneAndUpdateOptions<Item>
                {
                    ReturnDocument = ReturnDocument.After // Return the document after update
                });
                return updatedDocument;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
