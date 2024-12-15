using Microsoft.AspNetCore.Mvc;
using wink.Services;
using wink.Models;

namespace wink.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ItemService _itemService;

        //inject the service class
        public ItemController(ItemService itemService) =>
            _itemService = itemService;

        [HttpGet]
        public async Task<List<Item>> getItems() => await _itemService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Item>> Get(string id)
        {
            var item = await _itemService.GetAsync(id);

            if (item is null)
            {
                return NotFound();
            }

            return item;
        }

        [HttpPost]
        public async Task<IActionResult> addItem(Item item) {
            await _itemService.CreateAsync(item);
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPatch]
        public async Task<ActionResult<Item>> editItem (string id,int quantity)
        {
            var oldItem = await _itemService.GetAsync(id);
            if (oldItem is null)
            {
                return NotFound();
            }
            else if (oldItem.Quantity - quantity < 0) {
                return BadRequest(new { message = "This quantity is not available" });
            }
            var item = await _itemService.DecrementAsync(id, oldItem.Quantity - quantity);
            return item is null ? NotFound() : item;
        }

        [HttpGet("category")]
        public async Task<ActionResult<List<Item>>> getItemsByCategory(string category)
        {
            var list = await _itemService.GetByCategoryAsync(category.ToLower());
            if(list.Count() == 0)
            {
                return BadRequest(new { message = "Category not found" });
            }

            return list;
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> deleteItem(string id)
        {
            var item = await _itemService.GetAsync(id);
            if (item is null) { 
                return NotFound();
            }

            await _itemService.RemoveAsync(id);

            return NoContent();
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Item updatedItem)
        {
            var item = await _itemService.GetAsync(id);

            if (item is null)
            {
                return NotFound();
            }

            updatedItem.Id = item.Id;

            await _itemService.UpdateAsync(id, updatedItem);

            return NoContent();
        }

    }
}
