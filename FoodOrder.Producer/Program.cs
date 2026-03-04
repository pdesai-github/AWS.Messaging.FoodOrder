
namespace FoodOrder.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // Add AWS services - construct scoped SNS client so credentials are resolved fresh per request
            builder.Services.AddScoped<Amazon.SimpleNotificationService.IAmazonSimpleNotificationService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "us-east-1");
                var credentialProfileStoreChain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
                var profileName = configuration["AWS:Profile"] ?? "default";
                if (credentialProfileStoreChain.TryGetAWSCredentials(profileName, out var credentials))
                {
                    return new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient(credentials, region);
                }
                return new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient(region);
            });
            builder.Services.AddScoped<FoodOrder.Producer.Services.ISnsPublisher, FoodOrder.Producer.Services.SnsPublisher>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowLocalhost3000");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
