using Fintech.Api.Services;

namespace Fintech.Api.Middleware;
public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenService _tokenService)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
        if (!string.IsNullOrEmpty(token) && await _tokenService.IsTokenRevoked(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked" });
            return;
        }
        await _next(context);
    }
}