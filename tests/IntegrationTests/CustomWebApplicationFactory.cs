using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        
        // Configure the application to use SQLite for testing
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        
        // Pass the --sqlite-database argument to the application
        // Not working. How pass just a flag and not an argument?
        builder.UseSetting("Args:0", "--sqlite-database");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Provide a test JWT key so authentication middleware can start
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestOnlyJwtKeyForIntegrationTestingPurposes1234567890",
                ["Jwt:Issuer"] = "MythApiIssuer",
                ["Jwt:Audience"] = "MythApiAudience"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Add any additional service configuration for testing
        });
    }
}