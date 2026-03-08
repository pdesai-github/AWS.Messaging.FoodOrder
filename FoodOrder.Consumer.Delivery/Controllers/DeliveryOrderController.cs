using Microsoft.AspNetCore.Mvc;
using FoodOrder.Consumer.Delivery.Services;
using FoodOrder.Shared;
using System.Text.Json;

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
        public async Task<ActionResult<List<FoodOrderMessage>>> ReceiveMessages()
        {

            try
            {
                var response = await _sqsService.ReceiveMessagesAsync();

                if (response.Messages == null || response.Messages.Count == 0)
                {
                    return Ok(Array.Empty<FoodOrderMessage>());
                }

                List<FoodOrderMessage> messages = response.Messages.Select(m =>
                {
                    var snsNotification = ParseSnsNotification(m.Body);
                    
                    return new FoodOrderMessage()
                    {
                        ReceiptHandle = m.ReceiptHandle,
                        MessageId = snsNotification?.MessageId,
                        Body = snsNotification?.Message,
                        MessageAttributes = snsNotification?.MessageAttributes
                    };
                }).ToList();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from SQS");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to receive messages", details = ex.Message });
            }
        }

        private SnsNotification? ParseSnsNotification(string body)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var rawNotification = JsonSerializer.Deserialize<SnsNotificationRaw>(body, options);
                
                if (rawNotification == null)
                {
                    return null;
                }

                Dictionary<string, string>? convertedAttributes = null;
                if (rawNotification.MessageAttributes != null)
                {
                    convertedAttributes = rawNotification.MessageAttributes
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Value ?? string.Empty
                        );
                }

                // Parse the nested Message to extract the actual Body content
                string? actualBody = rawNotification.Message;
                if (!string.IsNullOrEmpty(rawNotification.Message))
                {
                    try
                    {
                        var nestedMessage = JsonSerializer.Deserialize<NestedFoodOrderMessage>(rawNotification.Message, options);
                        if (nestedMessage != null)
                        {
                            actualBody = nestedMessage.Body;
                            // Merge attributes from nested message if present
                            if (nestedMessage.MessageAttributes != null)
                            {
                                convertedAttributes ??= new Dictionary<string, string>();
                                foreach (var attr in nestedMessage.MessageAttributes)
                                {
                                    convertedAttributes[attr.Key] = attr.Value;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // If parsing fails, use the message as-is
                    }
                }
                
                return new SnsNotification
                {
                    MessageId = rawNotification.MessageId,
                    Message = actualBody,
                    MessageAttributes = convertedAttributes
                };
            }
            catch
            {
                return null;
            }
        }

        private class NestedFoodOrderMessage
        {
            public string? Body { get; set; }
            public Dictionary<string, string>? MessageAttributes { get; set; }
        }

        private class SnsNotificationRaw
        {
            public string? MessageId { get; set; }
            public string? Message { get; set; }
            public Dictionary<string, SnsMessageAttributeValue>? MessageAttributes { get; set; }
        }

        private class SnsNotification
        {
            public string? MessageId { get; set; }
            public string? Message { get; set; }
            public Dictionary<string, string>? MessageAttributes { get; set; }
        }

        private class SnsMessageAttributeValue
        {
            public string? Type { get; set; }
            public string? Value { get; set; }
        }

        /// <summary>
        /// Deletes a processed delivery order message from the SQS queue
        /// </summary>
        /// <param name="request">The receipt handle of the message to delete</param>
        [HttpDelete("messages")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMessage([FromBody] string receiptHandle)
        {
            if (string.IsNullOrWhiteSpace(receiptHandle))
                return BadRequest("ReceiptHandle is required.");

            try
            {
                await _sqsService.DeleteMessageAsync(receiptHandle);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message from SQS");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to delete message", details = ex.Message });
            }
        }

        /// <summary>
        /// Receives messages from the Dead Letter Queue (DLQ)
        /// </summary>
        [HttpGet("dlq/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<FoodOrderMessage>>> ReceiveDlqMessages()
        {
            try
            {
                var response = await _sqsService.ReceiveDlqMessagesAsync();

                if (response.Messages == null || response.Messages.Count == 0)
                {
                    return Ok(Array.Empty<FoodOrderMessage>());
                }

                List<FoodOrderMessage> messages = response.Messages.Select(m =>
                {
                    var snsNotification = ParseSnsNotification(m.Body);
                    
                    return new FoodOrderMessage()
                    {
                        ReceiptHandle = m.ReceiptHandle,
                        MessageId = snsNotification?.MessageId,
                        Body = snsNotification?.Message,
                        MessageAttributes = snsNotification?.MessageAttributes
                    };
                }).ToList();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from DLQ");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to receive DLQ messages", details = ex.Message });
            }
        }

        /// <summary>
        /// Redrive messages from DLQ back to the main queue
        /// </summary>
        /// <param name="maxMessages">Maximum number of messages to redrive per second</param>
        [HttpPost("dlq/redrive")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RedriveMessages([FromQuery] int maxMessages = 10)
        {
            try
            {
                await _sqsService.RedriveMessagesAsync(maxMessages);
                return Accepted(new { message = "Message redrive task started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redriving messages from DLQ");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Failed to redrive messages", details = ex.Message });
            }
        }
    }

    public class DeleteMessageRequest
    {
        public string ReceiptHandle { get; set; } = string.Empty;
    }
}
