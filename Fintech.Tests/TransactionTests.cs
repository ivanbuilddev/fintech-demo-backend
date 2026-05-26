using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Fintech.Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace Fintech.Tests;

[Collection("AccountCollection")]
public class TransactionTests
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TransactionTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
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
    public async Task GetTransactionById_ReturnsForbid_WhenTokenIsNotTheOwner()
    {
        var loginResponse = await TestHelper.Login(_client, "BobDemo");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var transactionId = Guid.Parse("B6666666-6666-6666-6666-666666666666");
        var response = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
    public async Task GetTransactionsByAccountId_ReturnsForbid_WhenTokenIsNotTheOwner()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var accountId = Guid.Parse("B4444444-4444-4444-4444-444444444444");
        var response = await _client.GetAsync($"/api/Transaction/transactions/{accountId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsTransfer()
    {
        var loginResponse = await TestHelper.Login(_client);
        var loginResponse2 = await TestHelper.Login(_client, "BobDemo");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey1");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("A3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("B4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        
        var responseSourceAccount = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.SourceAccountId}");
        var sourceAccount = await responseSourceAccount.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(sourceAccount);
        var sourceInitialBalance = sourceAccount.Balance;
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse2.Token);
        var responseDestinationAccount = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.DestinationAccountId}");
        var destinationAccount = await responseDestinationAccount.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(destinationAccount);
        var destinationInitialBalance = destinationAccount.Balance;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Equal(createTransactionRequest.SourceAccountId, transaction.SourceAccountId);
        Assert.Equal(createTransactionRequest.DestinationAccountId, transaction.DestinationAccountId);
        Assert.Equal(createTransactionRequest.Description, transaction.Description);
        Assert.Equal(createTransactionRequest.Amount, transaction.Amount);
        Assert.Equal(createTransactionRequest.Type, transaction.Type);

        var afterResponseSourceAccount = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.SourceAccountId}");
        sourceAccount = await afterResponseSourceAccount.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(sourceAccount);
        Assert.Equal(sourceInitialBalance - createTransactionRequest.Amount, sourceAccount.Balance);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse2.Token);
        var afterResponseDestinationAccount = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.DestinationAccountId}");
        destinationAccount = await afterResponseDestinationAccount.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(destinationAccount);
        Assert.Equal(destinationInitialBalance + createTransactionRequest.Amount, destinationAccount.Balance);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsForbid_WhenSourceAccountIsNotTheOwnerAndTypeIsTransfer()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey2");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("B4444444-4444-4444-4444-444444444444"), DestinationAccountId = Guid.Parse("A3333333-3333-3333-3333-333333333333"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsWithdrawal()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey3");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Withdrawal };
        
        var responseAccount = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.SourceAccountId}");
        var account = await responseAccount.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        var initialBalance = account.Balance;

        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transaction = await response.Content.ReadFromJsonAsync<Transaction>();
        Assert.NotNull(transaction);
        Assert.Equal(createTransactionRequest.SourceAccountId, transaction.SourceAccountId);
        Assert.Null(transaction.DestinationAccountId);
        Assert.Equal(createTransactionRequest.Description, transaction.Description);
        Assert.Equal(createTransactionRequest.Amount, transaction.Amount);
        Assert.Equal(createTransactionRequest.Type, transaction.Type);

        var afterAccountResponse = await _client.GetAsync($"/api/Account/account/{createTransactionRequest.SourceAccountId}");
        account = await afterAccountResponse.Content.ReadFromJsonAsync<Account>();
        Assert.NotNull(account);
        Assert.Equal(initialBalance - createTransactionRequest.Amount, account.Balance);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsForbid_WhenSourceAccountIsNotTheOwnerAndTypeIsWithdrawal()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey4");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("B4444444-4444-4444-4444-444444444444"), DestinationAccountId = Guid.Parse("A3333333-3333-3333-3333-333333333333"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Withdrawal };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenRequestIsValidAndTypeIsDeposit()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey5");
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
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKey6");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333331"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenIdempotencyKeyIsDuplicated()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        _client.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKeyDuplicated");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333331"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        var reponse = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));
        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsBadRequest_WhenIdempotencyKeyIsEmpty()
    {
        var loginResponse = await TestHelper.Login(_client);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333331"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ReturnsUnauthorized_WhenTokenIsInvalid()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "InvalidToken");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Transfer };
        var response = await _client.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransfer_ShouldReturn409_WhenConcurrencyConflict()
    {

        var mockService = new Mock<ITransactionService>();
        mockService
            .Setup(s => s.CreateTransactionAsync(It.IsAny<CreateTransactionRequest>(), It.IsAny<Guid>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var mockClient = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITransactionService>(_ => mockService.Object);
            });
        }).CreateClient();

        var loginResponse = await TestHelper.Login(mockClient);

        mockClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        mockClient.DefaultRequestHeaders.Add("Idempotency-Key", "TestIdempotencyKeyConflictKey");
        var createTransactionRequest = new CreateTransactionRequest { SourceAccountId = Guid.Parse("a3333333-3333-3333-3333-333333333333"), DestinationAccountId = Guid.Parse("b4444444-4444-4444-4444-444444444444"), Description = "Test Transaction", Amount = 100.00m, Type = TransactionType.Withdrawal };
        var response = await mockClient.PostAsync($"/api/Transaction/transactions", JsonContent.Create(createTransactionRequest));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTransaction_ReturnsTransaction_WhenTokenIsValidAndWhenIdIsValid()
    {
        var loginResponse = await TestHelper.Login(_client);
        var loginResponse2 = await TestHelper.Login(_client, "BobDemo");

        var accountId = Guid.Parse("B4444444-4444-4444-4444-444444444444");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse2.Token);
        var responseAccount = await _client.GetAsync($"/api/Account/account/{accountId}");
        var account = await responseAccount.Content.ReadFromJsonAsync<Account>();
        Assert.Equal(HttpStatusCode.OK, responseAccount.StatusCode);
        var initialBalance = account!.Balance;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        var transactionId = Guid.Parse("a5555555-5555-5555-5555-555555555555");
        var transactionBefore = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");
        var transactionDataBefore = await transactionBefore.Content.ReadFromJsonAsync<Transaction>();
        var amountBefore = transactionDataBefore!.Amount;
        Assert.Equal(HttpStatusCode.OK, transactionBefore.StatusCode);

        var response = await _client.DeleteAsync($"/api/Transaction/transactions/{transactionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseTransaction = await _client.GetAsync($"/api/Transaction/transaction/{transactionId}");
        Assert.Equal(HttpStatusCode.NotFound, responseTransaction.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse2.Token);
        responseAccount = await _client.GetAsync($"/api/Account/account/{accountId}");
        account = await responseAccount.Content.ReadFromJsonAsync<Account>();
        Assert.Equal(HttpStatusCode.OK, responseAccount.StatusCode);
        Assert.Equal(account!.Balance, initialBalance - amountBefore);
    }

    //TODO: Test Transfer/Deposit/Withdrawal using frozen accounts
}