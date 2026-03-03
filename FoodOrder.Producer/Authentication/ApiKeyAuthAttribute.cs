using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FoodOrder.Producer.Authentication
{
    /// <summary>
    /// API Key authentication attribute for securing endpoints
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyAuthAttribute>>();

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyValue))
            {
                logger.LogWarning("Request rejected: Missing API Key header");
                context.Result = new UnauthorizedObjectResult(new { error = "Missing API Key" });
                return;
            }

            var configuredApiKey = configuration["Authentication:ApiKey"];
            if (string.IsNullOrEmpty(configuredApiKey))
            {
                logger.LogError("API Key not configured in appsettings");
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return;
            }

            if (!apiKeyValue.ToString().Equals(configuredApiKey))
            {
                logger.LogWarning("Request rejected: Invalid API Key provided");
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid API Key" });
                return;
            }

            logger.LogInformation("Request authenticated successfully");
            await next();
        }
    }
}
