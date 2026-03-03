using Amazon.SQS;
using Amazon.SQS.Model;

namespace FoodOrder.Consumer.Delivery.Services
{
    public interface ISqsService
    {
        Task<ReceiveMessageResponse> ReceiveMessagesAsync(int maxMessages = 10, int waitTimeSeconds = 5);
        Task DeleteMessageAsync(string receiptHandle);
    }

    public class SqsService : ISqsService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqsService> _logger;

        public SqsService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsService> logger)
        {
            _sqsClient = sqsClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ReceiveMessageResponse> ReceiveMessagesAsync(int maxMessages = 10, int waitTimeSeconds = 5)
        {
            var queueUrl = GetQueueUrl();

            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = waitTimeSeconds,
                MessageAttributeNames = ["All"],
                AttributeNames = ["All"]
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);
            _logger.LogInformation("Received {Count} message(s) from SQS queue", response.Messages.Count);
            return response;
        }

        public async Task DeleteMessageAsync(string receiptHandle)
        {
            var queueUrl = GetQueueUrl();

            var request = new DeleteMessageRequest
            {
                QueueUrl = queueUrl,
                ReceiptHandle = receiptHandle
            };

            await _sqsClient.DeleteMessageAsync(request);
            _logger.LogInformation("Deleted message with receipt handle {ReceiptHandle}", receiptHandle);
        }

        private string GetQueueUrl()
        {
            var queueUrl = _configuration["AWS:SQS:QueueUrl"];
            if (string.IsNullOrEmpty(queueUrl))
                throw new InvalidOperationException("AWS SQS Queue URL is not configured.");
            return queueUrl;
        }
    }
}
