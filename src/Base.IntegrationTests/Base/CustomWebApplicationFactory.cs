using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Base.IntegrationTests.Base;

internal class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders(); // Remove default logging providers
            logging.AddConsole(); // Optional: Add console logging
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove hosted services for testing environment
            services.RemoveAll<IHostedService>();
        });
    }
}