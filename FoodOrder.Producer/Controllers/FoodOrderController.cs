using Microsoft.AspNetCore.Mvc;
using FoodOrder.Producer.Services;
using FoodOrder.Producer.Authentication;
using FoodOrder.Shared;

namespace FoodOrder.Producer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiKeyAuth]
    public class FoodOrderController : ControllerBase
    {
        private readonly ISnsPublisher _snsPublisher;
        private readonly ILogger<FoodOrderController> _logger;

        public FoodOrderController(ISnsPublisher snsPublisher, ILogger<FoodOrderController> logger)
        {
            _snsPublisher = snsPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new food delivery order and publishes it to AWS SNS
        /// </summary>
        /// <param name="request">The food order details</param>
        /// <returns>The message ID from SNS</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FoodOrderMessage>> CreateOrder([FromBody] FoodOrderMessage request)
        {
          
            try
            {
             
         

                var messageId = await _snsPublisher.PublishFoodOrderAsync(request);

                var response = new FoodOrderMessage { MessageId = messageId };

                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Failed to process order", details = ex.Message });
            }
        }
    }

}
