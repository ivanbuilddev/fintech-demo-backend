namespace Fintech.Api.Services;

public interface IOllamaService
{
    public Task<string> ChatAsync(string prompt);

    public IAsyncEnumerable<string> ChatYieldAsync(string prompt);
}