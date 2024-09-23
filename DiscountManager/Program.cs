using DiscountManager.Services;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

namespace DiscountManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder
                .Services
                .AddGrpc(opts => opts.EnableDetailedErrors = true);

            var connectionString = builder
                .Configuration
                .GetConnectionString(DiscountDbContext.CONNECTION_STRING_NAME);

            builder
                .Services
                .AddDbContext<DiscountDbContext>(options => options
                    .UseNpgsql(connectionString)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll));

            builder
                .Services
                .Configure<GeneratorOptions>(builder.Configuration.GetSection(GeneratorOptions.SECTION_NAME));
            
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            // Configure the HTTP request pipeline.
            app.MapGrpcService<DiscountService>()
                .WithHttpLogging(HttpLoggingFields.All);

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            await app.RunAsync();
        }
    }
}