using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using FoodOrder.Shared;
using System.Text.Json;

namespace FoodOrder.Consumer.Shop.Repositories
{
    public class ShopRepository : IShopRepository
    {
        AmazonSQSClient sqsClient = new AmazonSQSClient(RegionEndpoint.APSouth1);
        string shop1Url = "https://sqs.ap-south-1.amazonaws.com/509399622377/Shop1.fifo";
        string shop2Url = "https://sqs.ap-south-1.amazonaws.com/509399622377/Shop2.fifo";

        public async Task<List<FoodOrderMessage>> ReceiveMessages(string shopId)
        {
            string queueUrl = shopId.ToLower() == "shop-1" ? shop1Url : shop2Url;
            var request = new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                MessageAttributeNames = new List<string> { "All" }
            };
            ReceiveMessageResponse response = await sqsClient.ReceiveMessageAsync(request);

            List<FoodOrderMessage> foodOrderMessages = new List<FoodOrderMessage>();
            response.Messages?.ForEach(m =>
            {
                var snsNotification = ParseSnsNotification(m.Body);
                
                foodOrderMessages.Add(new FoodOrderMessage() 
                {
                    ReceiptHandle = m.ReceiptHandle,
                    MessageId = snsNotification?.MessageId,
                    Body = snsNotification?.Message,
                    MessageAttributes = snsNotification?.MessageAttributes
                });
            });

            return foodOrderMessages;
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

        public async Task DeleteMessage(string shopId, string receiptHandle)
        {
            try
            {
                string queueUrl = shopId == "shop-1" ? shop1Url : shop2Url;
                var request = new Amazon.SQS.Model.DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = receiptHandle
                };
                await sqsClient.DeleteMessageAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                throw;
            }
        }


    }
}
