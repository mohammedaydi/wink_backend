using Microsoft.AspNetCore.Mvc;
using wink.Services;
using wink.Models;
using MongoDB.Driver;
namespace wink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        private readonly ItemService _itemService;
        private readonly ItemCartService _itemCartService;

        //inject the service class
        public CartController(CartService cartService, ItemService itemService, ItemCartService itemCartService) {
            _cartService = cartService;
            _itemService = itemService;
            _itemCartService = itemCartService;
        }

        [HttpGet] 
        public async Task<List<Cart>> getCarts()
        {
            return await _cartService.GetAsync();
        }
        
        [HttpGet("{id:length(24)}")]
        public async Task<Cart?> Get(string id)
        {
            return await _cartService.GetAsync(id);
        }

        [HttpGet("user/{userId:length(24)}")]
        public async Task<ActionResult<List<Item>>> getCartItems(string userId)
        {
            var cart = await _cartService.GetByUserIdAsync(userId);

            if(cart == null)
            {
                return BadRequest("no cart found");
            }

            List<string> ids = new List<string>();
            foreach(var item in cart.Items)
            {
                if (item.ItemId is not null)
                {
                    ids.Add(item.ItemId);
                }
            }
            return await _itemService.GetItemsByIdsAsync(ids);
        }

        [HttpPost]
        public async Task<IActionResult> createCart(Cart cart)
        {
            if(cart == null || cart.UserId == null)
            {
                return BadRequest();
            }

            var check = await  _cartService.GetByUserIdAsync(cart.UserId);
            if (check != null) {
                return BadRequest("cart already exists for this user");
            }

            await _cartService.createAsync(cart);

            return CreatedAtAction(nameof(Get), new { id = cart.Id }, cart);
        }

        [HttpPatch("addItem/{userId:length(24)}")]
        public async Task<IActionResult> addCartItem(CartItem cartItem,string userId)
        {
            var cart = await _cartService.GetByUserIdAsync(userId);
            if (cart == null) {

                return BadRequest("No cart exists for this user");
            }

            await _cartService.addCartItem(userId, cartItem);
            var updatedCart = await _cartService.GetByUserIdAsync(userId);

            if (updatedCart == null)
            {
                return NotFound(new { Message = "Cart not found after update." });
            }

            updatedCart = await _itemCartService.updateTotalPrice(userId);

            return Ok(updatedCart);
        }
        

        [HttpPatch("editItem/{userId:length(24)}")]
        public async Task<IActionResult> updateCartItem(CartItem cartItem, string userId)
        {
            var cart = await _cartService.GetByUserIdAsync(userId);
            if (cart == null)
            {

                return BadRequest("No cart exists for this user");
            }

            try
            {
                await _cartService.updateCartItem(userId, cartItem);

            }
            catch (Exception ex) {
                return NotFound(new { Message = "requested item not found" });
            }


            var updatedCart = await _cartService.GetByUserIdAsync(userId);

            if (updatedCart == null)
            {
                return NotFound(new { Message = "Cart not found after update." });
            }

            updatedCart = await _itemCartService.updateTotalPrice(userId);
            return Ok(updatedCart);
        }

        [HttpPatch("removeItem/{userId:length(24)}")]
        public async Task<IActionResult> removeCartItem(CartItem cartItem, string userId)
        {
            var cart = await _cartService.GetByUserIdAsync(userId);
            if (cart == null)
            {

                return BadRequest("No cart exists for this user");
            }

            var updatedCart = await _cartService.removeCartItem(userId, cartItem);
            updatedCart = await _itemCartService.updateTotalPrice(userId);

            return Ok(updatedCart);
        }

        [HttpPatch("emptyCart/{userId:length(24)}")]
        public async Task<IActionResult> removeCartItem(string userId)
        {
            var cart = await _cartService.GetByUserIdAsync(userId);
            if (cart == null)
            {

                return BadRequest("No cart exists for this user");
            }

            var updatedCart = await _cartService.emptyCart(userId);
            updatedCart = await _itemCartService.updateTotalPrice(userId);

            return Ok(updatedCart);
        }


    }
}
