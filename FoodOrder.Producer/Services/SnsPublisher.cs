using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using FoodOrder.Shared;
using System.Text.Json;

namespace FoodOrder.Producer.Services
{
    public interface ISnsPublisher
    {
        Task<string> PublishFoodOrderAsync(FoodOrderMessage order);
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

        public async Task<string> PublishFoodOrderAsync(FoodOrderMessage order)
        {
            try
            {
                var topicArn = _configuration["AWS:SNS:TopicArn"];
                
                if (string.IsNullOrEmpty(topicArn))
                {
                    throw new InvalidOperationException("AWS SNS Topic ARN is not configured.");
                }

                var messageJson = JsonSerializer.Serialize(order);
                string shopId = order.MessageAttributes["ShopId"];

                var publishRequest = new PublishRequest
                {
                    TopicArn = topicArn,
                    Message = messageJson,
                    Subject = "Food Order Created",
                    MessageGroupId = shopId,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>()
                };

                publishRequest.MessageAttributes["ShopId"] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = shopId
                };

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
