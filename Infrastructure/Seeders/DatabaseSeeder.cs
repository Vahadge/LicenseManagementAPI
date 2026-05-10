using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Infrastructure.Seeders;

public class DatabaseSeeder
{
    private readonly IUserRepository  _userRepository;
    private readonly IPasswordHasher  _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<DatabaseSeeder> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger         = logger;
    }

    public async Task SeedAsync()
    {
        if (await _userRepository.AnyAsync()) return;

        var admin = new User
        {
            Username     = "admin",
            PasswordHash = _passwordHasher.Hash("Admin@123"),
            Role         = "Admin",
        };

        await _userRepository.CreateAsync(admin);
    }
}
