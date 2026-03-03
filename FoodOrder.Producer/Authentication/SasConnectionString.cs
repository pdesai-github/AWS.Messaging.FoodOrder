namespace FoodOrder.Producer.Authentication
{
    /// <summary>
    /// Represents an Azure Service Bus-like connection string with SAS token authentication
    /// </summary>
    public class SasConnectionString
    {
        public string Endpoint { get; set; } = string.Empty;
        public string SharedAccessKeyName { get; set; } = string.Empty;
        public string SharedAccessKey { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;

        /// <summary>
        /// Parses connection string in format:
        /// Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=keyname;SharedAccessKey=key;TopicName=topic-name
        /// </summary>
        public static SasConnectionString Parse(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

            var result = new SasConnectionString();
            var parts = connectionString.Split(';');

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                var keyValue = part.Split('=', 2);
                if (keyValue.Length != 2)
                    continue;

                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                switch (key)
                {
                    case "Endpoint":
                        result.Endpoint = value;
                        break;
                    case "SharedAccessKeyName":
                        result.SharedAccessKeyName = value;
                        break;
                    case "SharedAccessKey":
                        result.SharedAccessKey = value;
                        break;
                    case "TopicName":
                        result.TopicName = value;
                        break;
                }
            }

            if (string.IsNullOrEmpty(result.Endpoint) || 
                string.IsNullOrEmpty(result.SharedAccessKeyName) || 
                string.IsNullOrEmpty(result.SharedAccessKey) ||
                string.IsNullOrEmpty(result.TopicName))
            {
                throw new ArgumentException("Connection string is missing required components");
            }

            return result;
        }

        public override string ToString()
        {
            return $"Endpoint={Endpoint};SharedAccessKeyName={SharedAccessKeyName};SharedAccessKey={SharedAccessKey};TopicName={TopicName}";
        }
    }
}
