using Fintech.Api.DTOs;

namespace Fintech.Api.Services;
public interface IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request);
    public Task<bool> LogoutAsync(LogoutRequest request);
}