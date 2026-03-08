using Amazon.SQS;
using Amazon.SQS.Model;

namespace FoodOrder.Consumer.Delivery.Services
{
    public interface ISqsService
    {
        Task<ReceiveMessageResponse> ReceiveMessagesAsync();
        Task DeleteMessageAsync(string receiptHandle);
        Task<ReceiveMessageResponse> ReceiveDlqMessagesAsync();
        Task RedriveMessagesAsync(int maxMessages = 10);
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

        public async Task<ReceiveMessageResponse> ReceiveMessagesAsync()
        {
            var queueUrl = GetQueueUrl();

            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "ShopId" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);
           
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

        public async Task<ReceiveMessageResponse> ReceiveDlqMessagesAsync()
        {
            var dlqUrl = GetDlqUrl();

            var request = new ReceiveMessageRequest
            {
                QueueUrl = dlqUrl,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);
            return response;
        }

        public async Task RedriveMessagesAsync(int maxMessages = 10)
        {
            var queueUrl = GetQueueUrl();
            var dlqUrl = GetDlqUrl();

            var request = new StartMessageMoveTaskRequest
            {
                SourceArn = await GetQueueArnAsync(dlqUrl),
                DestinationArn = await GetQueueArnAsync(queueUrl),
                MaxNumberOfMessagesPerSecond = maxMessages
            };

            var response = await _sqsClient.StartMessageMoveTaskAsync(request);
            _logger.LogInformation("Started message redrive task: {TaskHandle}", response.TaskHandle);
        }

        private async Task<string> GetQueueArnAsync(string queueUrl)
        {
            var request = new GetQueueAttributesRequest
            {
                QueueUrl = queueUrl,
                AttributeNames = new List<string> { "QueueArn" }
            };

            var response = await _sqsClient.GetQueueAttributesAsync(request);
            return response.Attributes["QueueArn"];
        }

        private string GetQueueUrl()
        {
            var queueUrl = _configuration["AWS:SQS:QueueUrl"];
            if (string.IsNullOrEmpty(queueUrl))
                throw new InvalidOperationException("AWS SQS Queue URL is not configured.");
            return queueUrl;
        }

        private string GetDlqUrl()
        {
            var dlqUrl = _configuration["AWS:SQS:DlqUrl"];
            if (string.IsNullOrEmpty(dlqUrl))
                throw new InvalidOperationException("AWS SQS DLQ URL is not configured.");
            return dlqUrl;
        }
    }
}
