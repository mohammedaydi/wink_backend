using Microsoft.AspNetCore.Mvc;
using wink.Models;
using wink.Services;

namespace wink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController: ControllerBase
    {

        private readonly CartService _cartService;
        private readonly ItemService _itemService;
        private readonly UserService _userService;
        //private readonly ItemCartService _itemCartService;  

        public CheckoutController(CartService cartService, ItemService itemService, UserService userService)
        {
            _cartService = cartService;
            _itemService = itemService;
            _userService = userService;
        }

        [HttpPost("{id:length(24)}")]
        public async Task<IActionResult> checkout(string id)
        {
            if (id == null) {
                return BadRequest(new { message = "id must be provided" });
            }
            //get the user's items
            var cart = await _cartService.GetByUserIdAsync(id);
            if (cart is null)
            {
                return NotFound(new { message = "cart not found" });
            }

            List<string> ids = new List<string>();
            foreach (var item in cart.Items)
            {
                if (item.ItemId is not null)
                {
                    ids.Add(item.ItemId);
                }
            }
            var cartItemsInStock = await _itemService.GetItemsByIdsAsync(ids);

            var itemsInMyCart = cart.Items;
            bool quantityNotSufficient = false;

            decimal totalPrice = 0;
            //check the quantity if available
            foreach (var item in itemsInMyCart) {
                var cartItemInStock = cartItemsInStock.Find(x => x.Id == item.ItemId);
                if (cartItemInStock is null) {
                    return NotFound(new { message = $"{item.ItemId} item not found" });
                }

                if (cartItemInStock.Quantity < item.Quantity) {
                    quantityNotSufficient = true;
                    return BadRequest(new { message = $"{item.ItemId} quantity not sufficient" , availableQuantity= cartItemInStock.Quantity });
                }

                else
                {
                    cartItemInStock.Quantity -= item.Quantity;
                }

                totalPrice += (cartItemInStock.Price * item.Quantity);


            }

            //check if the user's balance is enough
            var user = await _userService.GetAsync(id);

            if(user is null)
            {
                return NotFound(new { message = "user not found" });
            }

            if(user.Balance < totalPrice)
            {
                return BadRequest(new { 
                    message = $"total is {totalPrice} but your balance is {user.Balance}",
                    total= totalPrice,
                    balance= user.Balance
                });
            }

            //here update the user's balance
            user.Balance -= totalPrice;
            await _userService.UpdateAsync(id, user);



            //update the quantities
            foreach (var item in cartItemsInStock)
            {
                if (item is null || item.Id is null) { continue; }
                await _itemService.DecrementAsync(item.Id, item.Quantity);
            }


            //empty the cart
            var cart_after = await _cartService.emptyCart(id);
            cart_after.Total = 0;


            //shape the data correctly

            var data = new { balance_after = user.Balance, items_after = cartItemsInStock, cart_after };

            //successfull checkout 
            return Ok(new { message = "checkout process completed successfully", data });
        }
    }
}
