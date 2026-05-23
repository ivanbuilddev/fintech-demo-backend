namespace Fintech.Api.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HashSet<string> _blacklist;

        public TokenBlacklistMiddleware(RequestDelegate next, HashSet<string> blacklist)
        {
            _next = next;
            _blacklist = blacklist;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            if (!string.IsNullOrEmpty(token) && _blacklist.Contains(token))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked" });
                return;
            }
            await _next(context);
        }
    }
}