using System.Net;
using System.Text;
using System.Text.Json;

namespace Fintech.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        var body = new { username = "AliceDemo" };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Auth/login", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
