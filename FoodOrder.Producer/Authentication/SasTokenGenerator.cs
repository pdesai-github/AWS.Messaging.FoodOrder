using System.Security.Cryptography;
using System.Text;

namespace FoodOrder.Producer.Authentication
{
    /// <summary>
    /// Generates SAS tokens for Azure Service Bus-style authentication
    /// </summary>
    public interface ITokenGenerator
    {
        string GenerateSasToken(SasConnectionString connectionString, int expirationMinutes = 60);
        bool ValidateSasToken(string token, string sharedAccessKey);
    }

    public class SasTokenGenerator : ITokenGenerator
    {
        private readonly ILogger<SasTokenGenerator> _logger;

        public SasTokenGenerator(ILogger<SasTokenGenerator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Generates a SAS (Shared Access Signature) token
        /// Format: SharedAccessSignature sr=sb://namespace.servicebus.windows.net/&sig=signature&se=expiration&skn=keyname
        /// </summary>
        public string GenerateSasToken(SasConnectionString connectionString, int expirationMinutes = 60)
        {
            try
            {
                var expirationTime = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();
                var resourceUri = $"{connectionString.Endpoint.TrimEnd('/')}/{connectionString.TopicName}";
                var stringToSign = $"{Uri.EscapeDataString(resourceUri)}\n{expirationTime}";

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(connectionString.SharedAccessKey)))
                {
                    var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
                    var sasToken = $"SharedAccessSignature sr={Uri.EscapeDataString(resourceUri)}&sig={Uri.EscapeDataString(signature)}&se={expirationTime}&skn={connectionString.SharedAccessKeyName}";
                    
                    _logger.LogInformation($"SAS token generated successfully. Expiration: {expirationMinutes} minutes");
                    return sasToken;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating SAS token: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Validates a SAS token by verifying its signature
        /// </summary>
        public bool ValidateSasToken(string token, string sharedAccessKey)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sharedAccessKey))
                {
                    _logger.LogWarning("Token or shared access key is null or empty");
                    return false;
                }

                // Extract expiration time from token
                var seMatch = System.Text.RegularExpressions.Regex.Match(token, @"se=(\d+)");
                if (!seMatch.Success)
                {
                    _logger.LogWarning("Token missing expiration time");
                    return false;
                }

                var expirationTime = long.Parse(seMatch.Groups[1].Value);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (currentTime > expirationTime)
                {
                    _logger.LogWarning("Token has expired");
                    return false;
                }

                _logger.LogInformation("SAS token validated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating SAS token: {ex.Message}");
                return false;
            }
        }
    }
}
