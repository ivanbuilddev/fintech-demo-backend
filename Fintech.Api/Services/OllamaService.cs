using System.Text;
using OllamaSharp;

namespace Fintech.Api.Services;

public class OllamaService : IOllamaService
{
    private readonly IOllamaApiClient _ollama;

    public OllamaService(IOllamaApiClient ollama)
    {
        _ollama = ollama;
        _ollama.SelectedModel = "qwen2.5-coder:3b";
    }

    public async Task<string> ChatAsync(string prompt)
    {
        var chat = new Chat(_ollama, "You are a financial analyst");
        var sb = new StringBuilder();
        await foreach (var token in chat.SendAsync(prompt))
        {
            sb.Append(token);
        }
        
        return sb.ToString();
    }

    public async IAsyncEnumerable<string> ChatYieldAsync(string prompt)
    {
        var chat = new Chat(_ollama, "You are a helpful assistant.");
        await foreach (var token in chat.SendAsync(prompt))
        {
            yield return token;
        }
    }
}

public class OllamaStubService : IOllamaService
{
    public async Task<string> ChatAsync(string prompt)
    {
        return await Task.FromResult("AI features are not available in this environment.");
    }

    public async IAsyncEnumerable<string> ChatYieldAsync(string prompt)
    {
        yield return "AI features are not available in this environment.";
        await Task.CompletedTask;
    }
}