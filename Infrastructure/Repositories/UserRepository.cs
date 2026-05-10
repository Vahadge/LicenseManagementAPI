using Dapper;
using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    public UserRepository(IDbConnectionFactory db) => _db = db;

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = _db.Create();
        return await conn.QuerySingleOrDefaultAsync<User>(
            "SELECT Id, Username, PasswordHash, Role, CreatedDate FROM dbo.Users WHERE Username = @Username",
            new { Username = username });
    }

    public async Task<bool> AnyAsync()
    {
        using var conn = _db.Create();
        return await conn.ExecuteScalarAsync<bool>("SELECT CAST(COUNT(1) AS BIT) FROM dbo.Users");
    }

    public async Task CreateAsync(User user)
    {
        using var conn = _db.Create();
        await conn.ExecuteAsync(
            "INSERT INTO dbo.Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role)",
            new { user.Username, user.PasswordHash, user.Role });
    }
}
