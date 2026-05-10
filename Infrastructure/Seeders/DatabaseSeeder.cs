using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Common.Helpers;
using LicenseManagementAPI.Domain.Entities;

namespace LicenseManagementAPI.Infrastructure.Seeders;

public class DatabaseSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IUserRepository userRepository, ILogger<DatabaseSeeder> logger)
    {
        _userRepository = userRepository;
        _logger         = logger;
    }

    public async Task SeedAsync()
    {
        if (await _userRepository.AnyAsync()) return;

        var admin = new User
        {
            Username     = "admin",
            PasswordHash = PasswordHelper.Hash("Admin@123"),
            Role         = "Admin",
        };

        await _userRepository.CreateAsync(admin);
        //_logger.LogInformation("Default admin user created. Username: admin  Password: Admin@123");
    }
}
