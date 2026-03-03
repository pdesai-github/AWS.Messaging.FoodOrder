using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

namespace FoodOrder.Producer.Services
{
    public interface ISnsPublisher
    {
        Task<string> PublishFoodOrderAsync(object order);
    }

    public class SnsPublisher : ISnsPublisher
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SnsPublisher> _logger;

        public SnsPublisher(
            IAmazonSimpleNotificationService snsClient, 
            IConfiguration configuration, 
            ILogger<SnsPublisher> logger)
        {
            _snsClient = snsClient;
            _configuration = configuration;
            _logger = logger;
            _logger.LogInformation("SnsPublisher initialized with local AWS profile");
        }

        public async Task<string> PublishFoodOrderAsync(object order)
        {
            try
            {
                var topicArn = _configuration["AWS:SNS:TopicArn"];
                
                if (string.IsNullOrEmpty(topicArn))
                {
                    throw new InvalidOperationException("AWS SNS Topic ARN is not configured.");
                }

                var messageJson = JsonSerializer.Serialize(order);

                var publishRequest = new PublishRequest
                {
                    TopicArn = topicArn,
                    Message = messageJson,
                    Subject = "Food Order Created"
                };

                // Add FIFO-required fields and message attributes if order is FoodOrderRequest
                if (order is FoodOrder.Producer.Models.FoodOrderRequest foodOrder)
                {
                    publishRequest.MessageGroupId = foodOrder.MessageGroupId;
                    publishRequest.MessageDeduplicationId = foodOrder.OrderId;

                    if (foodOrder.MessageAttributes != null && foodOrder.MessageAttributes.Count > 0)
                    {
                        foreach (var attribute in foodOrder.MessageAttributes)
                        {
                            var messageAttribute = new MessageAttributeValue
                            {
                                DataType = "String",
                                StringValue = string.Join(",", attribute.Value)
                            };
                            publishRequest.MessageAttributes.Add(attribute.Key, messageAttribute);
                        }
                    }
                }

                var response = await _snsClient.PublishAsync(publishRequest);
                
                _logger.LogInformation($"Message published successfully to SNS. MessageId: {response.MessageId}");
                
                return response.MessageId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing message to SNS: {ex.Message}");
                throw;
            }
        }
    }
}
