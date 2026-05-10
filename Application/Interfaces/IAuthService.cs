using LicenseManagementAPI.Application.DTOs;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
