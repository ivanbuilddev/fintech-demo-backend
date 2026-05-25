using Fintech.Api.DTOs;

namespace Fintech.Api.Services;
public interface IIdempotencyService
{
    public Task<bool> CreateAsync(string resourceKey, string requestPath, int responseStatusCode, string responseBody);
    public Task<IdempotencyDuplicateResponse?> IsDuplicateAsync(string resourceKey, string requestPath);
}