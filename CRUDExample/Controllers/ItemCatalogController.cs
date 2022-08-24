using CRUDExample.Infrastructure;
using CRUDExample.ViewModel;
using CRUDExample.Model;
using System.Net;

namespace CRUDExample.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ItemCatalogController : ControllerBase
    {
        private readonly ItemCatalogContext _itemsContext;

        public ItemCatalogController(ItemCatalogContext context)
        {
            _itemsContext = context;
        }

        //api/v1/[controller]/items?pageSize=1&pageIndex=2
        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(IEnumerable<Item>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<Item>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ItemsAsync([FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0, string? ids = null)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = await GetItemsByIdsAsync(ids);
                if (!items.Any())
                {
                    return BadRequest("ids value invalid. Must be comma-separated list of numbers");
                }
                return Ok(items);
            }

            var itemsCount = await _itemsContext.Items.LongCountAsync();
            var itemsOnPage = await _itemsContext.Items.OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var model = new PaginatedItemsViewModel<Item>(pageIndex, pageSize, itemsCount, itemsOnPage);
            return Ok(model);
        }

        private async Task<List<Item>> GetItemsByIdsAsync(string ids)
        {
            var numIds = ids.Split(',').Select(id => (Ok: int.TryParse(id, out int x), Value: x));
            if (!numIds.All(nid => nid.Ok))
            {
                return new List<Item>();
            }
            var idsToSelect = numIds.Select(val => val.Value);
            var items = await _itemsContext.Items.Where(ci => idsToSelect.Contains(ci.Id)).ToListAsync();
            return items;
        }

        [HttpGet]
        [Route("items/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(Item), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<Item>> ItemByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var item = await _itemsContext.Items.SingleOrDefaultAsync(ci => ci.Id == id);

            if (item != null)
            {
                return Ok(item);
            }
            return NotFound();
        }

        //GET api/v1/[controller]/items/withname/somename[?pageSize=5&pageIndex=1]
        [HttpGet]
        [Route("items/withname/{name:minlength(1)}")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<Item>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<PaginatedItemsViewModel<Item>>> GetItemByNameAsync(string name, [FromQuery] int pageSize = 10, [FromQuery] int pageIndex = 0)
        {
            var itemsCount =  await _itemsContext.Items.Where(x => x.Name.StartsWith(name)).LongCountAsync();
            var itemsOnPage = await _itemsContext.Items.Where(x => x.Name.StartsWith(name))
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();
            return new PaginatedItemsViewModel<Item>(pageIndex, pageSize, itemsCount, itemsOnPage);
        }
    }
}
