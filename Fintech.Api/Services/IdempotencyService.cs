using Fintech.Api.Data;
using Fintech.Api.DTOs;
using Fintech.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Fintech.Api.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(AppDbContext dbContext, ILogger<IdempotencyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> CreateAsync(string resourceKey, string requestPath, int responseStatusCode, string responseBody)
    {
        _logger.LogInformation("Creating key {key} in resource {resource}.", resourceKey, requestPath);
        var idempotencyKey = new IdempotencyKey
        {
            ResourceKey = resourceKey,
            RequestPath = requestPath,
            ResponseStatusCode = responseStatusCode,
            ResponseBody = responseBody,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
        };

        _dbContext.IdempotencyKeys.Add(idempotencyKey);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Key created.");
        return true;
    }

    public async Task<IdempotencyDuplicateResponse?> IsDuplicateAsync(string resourceKey, string requestPath)
    {
        _logger.LogInformation("Checking for key {key} in resource {resource}.", resourceKey, requestPath);
        var idempotencyKey = await _dbContext.IdempotencyKeys.FirstOrDefaultAsync(item => item.ResourceKey == resourceKey && item.RequestPath == requestPath);

        if (idempotencyKey == null)
        {
            _logger.LogInformation("Key not found.");
            return null;
        }

        if (idempotencyKey.ExpiresAtUtc < DateTime.UtcNow)
        {
            _dbContext.IdempotencyKeys.Remove(idempotencyKey);
            await _dbContext.SaveChangesAsync();
            return null;
        }

        _logger.LogInformation("Key found.");
        return new IdempotencyDuplicateResponse{ ReponseBody = idempotencyKey.ResponseBody, StatusCode = idempotencyKey.ResponseStatusCode };
    }
}