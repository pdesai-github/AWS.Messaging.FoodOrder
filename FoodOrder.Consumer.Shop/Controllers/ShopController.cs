using FoodOrder.Consumer.Shop.Services;
using FoodOrder.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Consumer.Shop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<ActionResult<List<FoodOrderMessage>>> Get(string shopId)
        {
            var messages = await _shopService.ReceiveMessages(shopId);
            return Ok(messages);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string shopId, [FromBody] DeleteMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ReceiptHandle))
            {
                return BadRequest("ReceiptHandle is required.");
            }

            await _shopService.DeleteMessage(shopId, request.ReceiptHandle);
            return NoContent();
        }
    }

    public class DeleteMessageRequest
    {
        public string ReceiptHandle { get; set; } = string.Empty;
    }
}
