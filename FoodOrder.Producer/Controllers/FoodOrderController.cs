using Microsoft.AspNetCore.Mvc;
using FoodOrder.Producer.Models;
using FoodOrder.Producer.Services;
using FoodOrder.Producer.Authentication;

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
        public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] FoodOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest("Order request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return BadRequest("Customer name is required");
            }

            if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
            {
                return BadRequest("Delivery address is required");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("Order must contain at least one item");
            }

            try
            {
                _logger.LogInformation($"Processing order {request.OrderId} for customer {request.CustomerName}");

                // Initialize message attributes if not already set
                if (request.MessageAttributes == null)
                {
                    request.MessageAttributes = new Dictionary<string, List<string>>();
                }

                // Ensure ShopId is set in message attributes
                if (!request.MessageAttributes.ContainsKey("ShopId"))
                {
                    request.MessageAttributes["ShopId"] = new List<string> { "shop-1" };
                }

                var messageId = await _snsPublisher.PublishFoodOrderAsync(request);

                var response = new OrderResponse
                {
                    OrderId = request.OrderId,
                    MessageId = messageId,
                    Status = "Order received and published to SNS",
                    CreatedAt = DateTime.UtcNow
                };

                return CreatedAtAction(nameof(CreateOrder), new { orderId = request.OrderId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { error = "Failed to process order", details = ex.Message });
            }
        }
    }

    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
