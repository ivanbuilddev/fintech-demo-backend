using Fintech.Api.Models;

namespace Fintech.Api.Services;
public interface IUserService
{
    public Task<User?> GetUserByIdAsync(Guid userId);
    public Task<User?> GetByUsernameAsync(string username);
    public Task<User> CreateUserAsync(string username, UserRoles role);
    public Task<IEnumerable<User>> GetAllUsersAsync();
}