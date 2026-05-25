using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Fintech.Api.DTOs;
using Fintech.Api.Models;

namespace Fintech.Tests;

public class AccountTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AccountTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAccountById_ReturnsAccount_WhenTokenIsValidAndWhenIdIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var response = await _client.GetAsync($"/api/Account/account/{accountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        Assert.Equal(accountId, account.Id);
    }
    [Fact]
    public async Task GetAccountById_ReturnsNotFound_WhenTokenIsValidAndWhenIdIsInvalid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333331");
        var response = await _client.GetAsync($"/api/Account/account/{accountId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ReturnsBadRequest_WhenTokenIsValidAndWhenIdIsEmpty()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var response = await _client.GetAsync($"/api/Account/account/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ReturnsUnathorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var response = await _client.GetAsync($"/api/Account/account/{accountId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_ReturnsForbid_WhenTokenIsNotTheOwner()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("B4444444-4444-4444-4444-444444444444");
        var response = await _client.GetAsync($"/api/Account/account/{accountId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountsByUserId_ReturnsAccounts_WhenTokenIsValidAndWhenIdIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var response = await _client.GetAsync($"/api/Account/accounts/{userId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var accounts = await response.Content.ReadFromJsonAsync<IEnumerable<Account>>();
        Assert.NotNull(accounts);
        Assert.NotEmpty(accounts);
    }

    [Fact]
    public async Task GetAccountsByUserId_ReturnsNotFound_WhenTokenIsValidAndWhenIdIsInvalid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var userId = Guid.Parse("a1111111-1111-1111-1111-111111111112");
        var response = await _client.GetAsync($"/api/Account/accounts/{userId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountsByUserId_ReturnsBadRequest_WhenTokenIsValidAndWhenIdIsEmpty()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var response = await _client.GetAsync($"/api/Account/accounts/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountsByUserId_ReturnsUnathorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var response = await _client.GetAsync($"/api/Account/accounts/{userId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountsByUserId_ReturnsForbid_WhenTokenIsNotTheOwner()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var userId = Guid.Parse("B2222222-2222-2222-2222-222222222222");
        var response = await _client.GetAsync($"/api/Account/accounts/{userId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_ReturnsAccount_WhenTokenIsValidAndWhenRequestIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createAccountRequest = new CreateAccountRequest { Name = "Test Account", Currency = "EUR" };
        var response = await _client.PostAsync($"/api/Account/accounts", JsonContent.Create(createAccountRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        Assert.Equal(createAccountRequest.Name, account.Name);
        Assert.Equal(loginResponse.User.Id, account.UserId);
        Assert.Equal(createAccountRequest.Currency, account.Currency);
    }

    [Fact]
    public async Task CreateAccount_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var createAccountRequest = new CreateAccountRequest { Name = "Test Account", Currency = "EUR" };
        var response = await _client.PostAsync($"/api/Account/accounts", JsonContent.Create(createAccountRequest));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_ReturnsAccount_WhenTokenIsValidAndWhenRequestIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var updateAccountRequest = new UpdateAccountRequest { Name = "Test Account Updated", IsActive = false };
        var response = await _client.PutAsync($"/api/Account/accounts/{accountId}", JsonContent.Create(updateAccountRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        Assert.Equal(updateAccountRequest.Name, account.Name);
        Assert.Equal(updateAccountRequest.IsActive, account.IsActive);
    }

    [Fact]
    public async Task UpdateAccount_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var updateAccountRequest = new UpdateAccountRequest { Name = "Test Account Updated", IsActive = false };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var response = await _client.PutAsync($"/api/Account/accounts/{accountId}", JsonContent.Create(updateAccountRequest));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAccount_ReturnsForbid_WhenTokenIsNotTheOwner()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("B4444444-4444-4444-4444-444444444444");
        var updateAccountRequest = new UpdateAccountRequest { Name = "Test Account Updated", IsActive = false };
        var response = await _client.PutAsync($"/api/Account/accounts/{accountId}", JsonContent.Create(updateAccountRequest));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}