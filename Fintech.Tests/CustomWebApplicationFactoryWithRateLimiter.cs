using Microsoft.AspNetCore.Hosting;

namespace Fintech.Tests;

public class CustomWebApplicationFactoryWithRateLimiter : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Production");
    }
}