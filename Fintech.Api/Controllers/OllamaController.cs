using Fintech.Api.DTOs;
using Fintech.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OllamaController : CustomControllerBase
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<OllamaController> _logger;

    public OllamaController(IOllamaService ollamaService, ILogger<OllamaController> logger)
    {
        _ollamaService = ollamaService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] OllamaChatRequest prompt)
    {
        _logger.LogInformation("Chat requested.");
        var response = await _ollamaService.ChatAsync(prompt.Message);
        _logger.LogInformation("Chat completed.");
        return Ok(response);
    }

    [HttpPost("chat/stream")]
    public async Task ChatStream([FromBody] OllamaChatRequest prompt)
    {
        _logger.LogInformation("ChatStream requested.");
        Response.StatusCode = 200;
        Response.ContentType = "text/event-stream";

        await foreach (var token in _ollamaService.ChatYieldAsync(prompt.Message))
        {
            await Response.WriteAsync(token);
            await Response.Body.FlushAsync(); // push each token immediately
        }
        _logger.LogInformation("ChatStream completed.");
    }
}