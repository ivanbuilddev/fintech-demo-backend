using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Fintech.Tests;

public class TransactionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TransactionTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactionById_ReturnsTransaction_WhenTokenIsValidAndWhenIdIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var transactionId = Guid.Parse("a5555555-5555-5555-5555-555555555555");
        var response = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Equal(transactionId, transaction.Id);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsNotFound_WhenTokenIsValidAndWhenIdIsInvalid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var transactionId = Guid.Parse("a5555555-5555-5555-5555-555555555550");
        var response = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionById_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var transactionId = Guid.Parse("a5555555-5555-5555-5555-555555555555");
        var response = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionsByAccountId_ReturnsTransactions_WhenTokenIsValidAndWhenIdIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var response = await _client.GetAsync($"/api/Transaction/transactions/{accountId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>();
        Assert.NotNull(transactions);
        Assert.NotEmpty(transactions);
    }

    [Fact]
    public async Task GetTransactionsByAccountId_ReturnsNotFound_WhenTokenIsValidAndWhenIdIsInvalid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333331");
        var response = await _client.GetAsync($"/api/Transaction/transactions/{accountId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactionsByAccountId_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var accountId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        var response = await _client.GetAsync($"/api/Transaction/transactions/{accountId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsTransfer()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Equal(createTransactionRequest.SourceAccountId, transaction.SourceAccountId);
        Assert.Equal(createTransactionRequest.DestinationAccountId, transaction.DestinationAccountId);
        Assert.Equal(createTransactionRequest.Description, transaction.Description);
        Assert.Equal(createTransactionRequest.Amount, transaction.Amount);
        Assert.Equal(createTransactionRequest.Type, transaction.Type);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsWithdrawal()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Withdrawal };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Equal(createTransactionRequest.SourceAccountId, transaction.SourceAccountId);
        Assert.Null(transaction.DestinationAccountId);
        Assert.Equal(createTransactionRequest.Description, transaction.Description);
        Assert.Equal(createTransactionRequest.Amount, transaction.Amount);
        Assert.Equal(createTransactionRequest.Type, transaction.Type);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsDeposit()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Deposit };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Null(transaction.SourceAccountId);
        Assert.Equal(createTransactionRequest.DestinationAccountId, transaction.DestinationAccountId);
        Assert.Equal(createTransactionRequest.Description, transaction.Description);
        Assert.Equal(createTransactionRequest.Amount, transaction.Amount);
        Assert.Equal(createTransactionRequest.Type, transaction.Type);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsNotFound_WhenTokenIsValidAndWhenAccountIsInvalid()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333331"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}