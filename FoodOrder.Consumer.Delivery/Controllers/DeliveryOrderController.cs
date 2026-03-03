using Microsoft.AspNetCore.Mvc;
using FoodOrder.Consumer.Delivery.Services;

namespace FoodOrder.Consumer.Delivery.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryOrderController : ControllerBase
    {
        private readonly ISqsService _sqsService;
        private readonly ILogger<DeliveryOrderController> _logger;

        public DeliveryOrderController(ISqsService sqsService, ILogger<DeliveryOrderController> logger)
        {
            _sqsService = sqsService;
            _logger = logger;
        }

        /// <summary>
        /// Receives pending delivery orders from the SQS queue
        /// </summary>
        /// <param name="maxMessages">Maximum number of messages to retrieve (1–10)</param>
        /// <param name="waitTimeSeconds">Long-polling wait time in seconds (0–20)</param>
        [HttpGet("messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReceiveMessages(
            [FromQuery] int maxMessages = 10,
            [FromQuery] int waitTimeSeconds = 5)
        {
            if (maxMessages < 1 || maxMessages > 10)
                return BadRequest("maxMessages must be between 1 and 10.");

            if (waitTimeSeconds < 0 || waitTimeSeconds > 20)
                return BadRequest("waitTimeSeconds must be between 0 and 20.");

            try
            {
                var response = await _sqsService.ReceiveMessagesAsync(maxMessages, waitTimeSeconds);

                var messages = response.Messages.Select(m => new
                {
                    m.MessageId,
                    m.ReceiptHandle,
                    m.Body,
                    Attributes = m.Attributes,
                    MessageAttributes = m.MessageAttributes
                });

                return Ok(new { count = response.Messages.Count, messages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from SQS");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to receive messages", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a processed delivery order message from the SQS queue
        /// </summary>
        /// <param name="request">The receipt handle of the message to delete</param>
        [HttpDelete("messages")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMessage([FromBody] DeleteMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ReceiptHandle))
                return BadRequest("ReceiptHandle is required.");

            try
            {
                await _sqsService.DeleteMessageAsync(request.ReceiptHandle);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message from SQS");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to delete message", details = ex.Message });
            }
        }
    }

    public class DeleteMessageRequest
    {
        public string ReceiptHandle { get; set; } = string.Empty;
    }
}
