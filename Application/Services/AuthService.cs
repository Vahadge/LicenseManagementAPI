using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LicenseManagementAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration  _configuration;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IConfiguration configuration)
    {
        _userRepository  = userRepository;
        _passwordHasher  = passwordHasher;
        _configuration   = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var jwtSettings = _configuration.GetSection("Jwt");
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var expiry = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryHours"] ?? "8"));
        var token  = new JwtSecurityToken(
            issuer:             jwtSettings["Issuer"],
            audience:           jwtSettings["Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds);

        return new LoginResponse
        {
            Token    = new JwtSecurityTokenHandler().WriteToken(token),
            Username = user.Username,
            Role     = user.Role,
        };
    }
}
