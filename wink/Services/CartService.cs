using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using wink.Models;

namespace wink.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        public CartService(
          IOptions<WinkDatabaseSettings> winkDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                winkDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                winkDatabaseSettings.Value.DatabaseName);

            _cartCollection = mongoDatabase.GetCollection<Cart>(
                winkDatabaseSettings.Value.CartsCollectionName);
            
        }

        public async Task<List<Cart>> GetAsync() =>
        await _cartCollection.Find(_ => true).ToListAsync();
        public async Task<Cart?> GetAsync(string id) =>
        await _cartCollection.Find(x => x.Id == id).FirstOrDefaultAsync<Cart>();

        public async Task<Cart?> GetByUserIdAsync(string id) =>
        await _cartCollection.Find(x => x.UserId == id).FirstOrDefaultAsync<Cart>();

        public async Task UpdateAsync(string id, Cart updatedCart) =>
             await _cartCollection.ReplaceOneAsync(x => x.Id == id, updatedCart);

        public async Task createAsync(Cart cart) => await _cartCollection.InsertOneAsync(cart);

        public async Task addCartItem(string userId, CartItem cartItem)
        {
            var filter = Builders<Cart>.Filter.Eq((cart) => cart.UserId, userId);
            var update = Builders<Cart>.Update.Push(c => c.Items, cartItem);

            var result = await _cartCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Cart not found or item not added.");
            }
        }

        public async Task updateCartItem(string userId, CartItem cartItem)
        {
            /* another way to use filters
             var filters = new List<FilterDefinition<Cart>>
            {
                Builders<Cart>.Filter.Eq(c => c.Id, cartId)
            };

            if (!string.IsNullOrEmpty(itemId))
            {
                filters.Add(Builders<Cart>.Filter.ElemMatch(c => c.Items, i => i.ItemId == itemId));
            }

            var filter = Builders<Cart>.Filter.And(filters);
             */


            var filter = Builders<Cart>.Filter.And(Builders<Cart>.Filter.Eq((cart) => cart.UserId, userId),
                Builders<Cart>.Filter.ElemMatch((cart) => cart.Items, i => i.ItemId == cartItem.ItemId));
            var update = Builders<Cart>.Update.Set("Items.$.Quantity", cartItem.Quantity);

            var result = await _cartCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Cart not found or item not added.");
            }
        }

        public async Task<Cart> removeCartItem(string userId, CartItem cartItem)
        {
            var filter = Builders<Cart>.Filter.Eq((cart) => cart.UserId, userId);
            var update = Builders<Cart>.Update.PullFilter(cart => cart.Items, item => item.ItemId == cartItem.ItemId);

            var cartAfterEdit = await _cartCollection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Cart>
            {
                ReturnDocument = ReturnDocument.After
            });

            return cartAfterEdit;
        }

        public async Task<Cart> emptyCart(string userId)
        {
            var filter = Builders<Cart>.Filter.Eq((cart) => cart.UserId, userId);
            var update = Builders<Cart>.Update.PullFilter(cart => cart.Items, _ => true);

            var cartAfterEdit = await _cartCollection.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<Cart>
            {
                ReturnDocument = ReturnDocument.After
            });

            return cartAfterEdit;
        }

        public async Task<List<String>?> getCartItems(string userId){
            var cart = await _cartCollection.Find(x => x.UserId == userId).FirstOrDefaultAsync();

            if(cart is null || cart.Items is null)
            {
                return null;
            }

            var itemsInCart = cart.Items.Select(x=> x.ItemId).ToList();
            return itemsInCart;    
        }


       

    }
}
