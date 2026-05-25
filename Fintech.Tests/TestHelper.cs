using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Fintech.Api.DTOs;

namespace Fintech.Tests;

public static class TestHelper
{
    public static async Task<LoginResponse> Login(HttpClient client)
    {
        var body = new { username = "AliceDemo" };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/Auth/login", content);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return loginResponse!;
    }
}