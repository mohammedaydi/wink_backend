using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using wink.Models;

namespace wink.Services
{
    public class ItemCartService
    {
        private readonly CartService _cartService;
        private readonly ItemService _itemService;

        public ItemCartService(CartService cartService, ItemService itemService)
        {
            _cartService = cartService;
            _itemService = itemService;
        }

        public async Task<Cart?> updateTotalPrice(string userId)
        {
            //get cart and its items
            var cart = await _cartService.GetByUserIdAsync(userId);
            if (cart is null || cart.Id is null)
            {
                return null;
            }

            var items = await _cartService.getCartItems(userId);

            if(items is null)
            {
                return null;
            }

            //get items in stock
            var itemsInStock = await _itemService.GetItemsByIdsAsync(items);

            //calculate total price
            decimal totalPrice = 0;
            var itemsInCart = cart.Items;

            foreach (var item in itemsInCart) {
                var itemInStock = itemsInStock.Find(x => x.Id == item.ItemId);
                if (itemInStock is null)
                {
                    continue;
                }
                totalPrice += itemInStock.Price * item.Quantity;
            }

            //update the cart
            cart.Total = totalPrice;
            await _cartService.UpdateAsync(cart.Id, cart);

            return cart;
        }
    }
}
