using Fintech.Api.Data;

namespace Fintech.Api.Workers;

public class DatabaseCapWorker : BackgroundService
{
    private readonly ILogger<DatabaseCapWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const int MaxAccounts = 10;
    private const int MaxTransactions = 100;

    public DatabaseCapWorker(ILogger<DatabaseCapWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                _logger.LogInformation("Database cap worker ran successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database cap worker failed.");
            }
        }
    }
}