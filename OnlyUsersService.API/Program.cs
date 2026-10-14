using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlyUsersService.API.Data;

namespace OnlyUsersService.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Auto-create database, apply EF Core migrations, and seed default roles and users
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Initializing database 'OnlyUsersService_DB'...");
                    var dbContext = services.GetRequiredService<OnlyUsersDbContext>();

                    // Ensure database is created and all pending EF Core migrations are applied
                    await dbContext.Database.MigrateAsync();

                    // Seed initial roles (Admin, User) and default administrative accounts
                    await DbInitializer.SeedAsync(dbContext, logger);

                    logger.LogInformation("Database 'OnlyUsersService_DB' initialized and seeded successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during database migration or initialization: {Message}", ex.Message);
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                        options.Limits.MaxResponseBufferSize = null;
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
