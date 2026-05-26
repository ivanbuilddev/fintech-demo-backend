namespace Fintech.Tests;

[CollectionDefinition("AccountCollection")]
public class AccountCollection : ICollectionFixture<CustomWebApplicationFactory> { }

[CollectionDefinition("TransactionCollection")]
public class TransactionCollection : ICollectionFixture<CustomWebApplicationFactory> { }