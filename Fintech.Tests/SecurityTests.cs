using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintech.Api.DTOs;

namespace Fintech.Tests;

public class SecurityTests:  IClassFixture<CustomWebApplicationFactoryWithRateLimiter>
{
    private readonly CustomWebApplicationFactoryWithRateLimiter _factory;

    public SecurityTests(CustomWebApplicationFactoryWithRateLimiter factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ShouldReturn429_WhenRateLimitIsExceeded()
    {
        var client = _factory.CreateClient();

        var loginResponse = await TestHelper.Login(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        for (int i = 0; i < 100; i++)
            await client.GetAsync($"/api/Account/account/{accountId}");

        var response = await client.GetAsync($"/api/Account/account/{accountId}");

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }
}