using LicenseManagementAPI.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LicenseManagementAPI.Infrastructure.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    public IDbConnection Create() => new SqlConnection(_connectionString);
}
