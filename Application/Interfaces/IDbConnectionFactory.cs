using System.Data;

namespace LicenseManagementAPI.Application.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}
