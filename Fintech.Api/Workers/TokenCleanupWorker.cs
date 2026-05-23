using Fintech.Api.Services;

namespace Fintech.Api.Workers
{
    public class TokenCleanupWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public TokenCleanupWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanUpTokenAsync();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanUpTokenAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            await tokenService.CleanUpTokenAsync();
        }
    }
}