using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodOrder.Producer.Authentication
{
    /// <summary>
    /// Token-based authentication attribute using Azure Service Bus-style SAS tokens
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SasTokenAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string BearerScheme = "Bearer";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SasTokenAuthAttribute>>();
            var tokenGenerator = context.HttpContext.RequestServices.GetRequiredService<ITokenGenerator>();

            // Check for Authorization header
            if (!context.HttpContext.Request.Headers.TryGetValue(AuthorizationHeaderName, out var authHeader))
            {
                logger.LogWarning("Request rejected: Missing Authorization header");
                context.Result = new UnauthorizedObjectResult(new { error = "Missing Authorization header" });
                return;
            }

            var authValue = authHeader.ToString();
            if (!authValue.StartsWith(BearerScheme, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Request rejected: Invalid Authorization scheme");
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid Authorization scheme. Use Bearer token." });
                return;
            }

            // Extract token
            var token = authValue.Substring(BearerScheme.Length).Trim();

            try
            {
                // Get shared access key from configuration
                var connectionString = configuration["Azure:ServiceBus:ConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogError("Connection string not configured");
                    context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    return;
                }

                var sasConnString = SasConnectionString.Parse(connectionString);

                // Validate token
                if (!tokenGenerator.ValidateSasToken(token, sasConnString.SharedAccessKey))
                {
                    logger.LogWarning("Request rejected: Invalid or expired token");
                    context.Result = new UnauthorizedObjectResult(new { error = "Invalid or expired token" });
                    return;
                }

                logger.LogInformation("Request authenticated successfully with SAS token");
                await next();
            }
            catch (Exception ex)
            {
                logger.LogError($"Token validation error: {ex.Message}");
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
