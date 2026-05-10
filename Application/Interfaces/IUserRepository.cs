using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> AnyAsync();
    Task CreateAsync(User user);
}
