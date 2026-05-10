using Dapper;
using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Domain.Enums;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LicenseManagementAPI.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly string _connectionString;

    public DoctorRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorListRequest request)
    {
        using var conn = CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Search", string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim());
        parameters.Add("@Status", string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim());
        parameters.Add("@PageNumber", request.PageNumber);
        parameters.Add("@PageSize", request.PageSize);

        using var multi = await conn.QueryMultipleAsync(
            "sp_GetDoctors", parameters, commandType: CommandType.StoredProcedure);

        var doctors = (await multi.ReadAsync<DoctorDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<DoctorDto>
        {
            Data = doctors,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<DoctorDto?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();

        const string sql = """
            SELECT
                Id,
                FullName,
                Email,
                Specialization,
                LicenseNumber,
                LicenseExpiryDate,
                CASE
                    WHEN [Status] = 'Suspended' THEN 'Suspended'
                    WHEN LicenseExpiryDate < CAST(GETDATE() AS DATE) THEN 'Expired'
                    ELSE 'Active'
                END AS [Status],
                CreatedDate
            FROM Doctors
            WHERE Id = @Id AND IsDeleted = 0
            """;

        return await conn.QuerySingleOrDefaultAsync<DoctorDto>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(CreateDoctorRequest request)
    {
        using var conn = CreateConnection();

        var status = request.LicenseExpiryDate.Date < DateTime.Today
            ? DoctorStatus.Expired
            : DoctorStatus.Active;

        const string sql = """
            INSERT INTO Doctors (FullName, Email, Specialization, LicenseNumber, LicenseExpiryDate, Status, CreatedDate)
            OUTPUT INSERTED.Id
            VALUES (@FullName, @Email, @Specialization, @LicenseNumber, @LicenseExpiryDate, @Status, @CreatedDate)
            """;

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            request.FullName,
            request.Email,
            request.Specialization,
            request.LicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate.Date,
            Status = status,
            CreatedDate = DateTime.UtcNow
        });
    }

    public async Task UpdateAsync(int id, UpdateDoctorRequest request)
    {
        using var conn = CreateConnection();

        const string sql = """
            UPDATE Doctors
            SET FullName          = @FullName,
                Email             = @Email,
                Specialization    = @Specialization,
                LicenseNumber     = @LicenseNumber,
                LicenseExpiryDate = @LicenseExpiryDate
            WHERE Id = @Id AND IsDeleted = 0
            """;

        await conn.ExecuteAsync(sql, new
        {
            request.FullName,
            request.Email,
            request.Specialization,
            request.LicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate.Date,
            Id = id
        });
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        using var conn = CreateConnection();

        await conn.ExecuteAsync(
            "UPDATE Doctors SET [Status] = @Status WHERE Id = @Id AND IsDeleted = 0",
            new { Status = status, Id = id });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = CreateConnection();

        await conn.ExecuteAsync(
            "UPDATE Doctors SET IsDeleted = 1 WHERE Id = @Id",
            new { Id = id });
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var conn = CreateConnection();

        return await conn.ExecuteScalarAsync<bool>(
            "SELECT CAST(COUNT(1) AS BIT) FROM Doctors WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id });
    }

    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeId = null)
    {
        using var conn = CreateConnection();

        var sql = "SELECT CAST(COUNT(1) AS BIT) FROM Doctors WHERE LicenseNumber = @LicenseNumber AND IsDeleted = 0";
        if (excludeId.HasValue)
            sql += " AND Id != @ExcludeId";

        return await conn.ExecuteScalarAsync<bool>(sql, new { LicenseNumber = licenseNumber, ExcludeId = excludeId });
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        using var conn = CreateConnection();

        var sql = "SELECT CAST(COUNT(1) AS BIT) FROM Doctors WHERE Email = @Email AND IsDeleted = 0";
        if (excludeId.HasValue)
            sql += " AND Id != @ExcludeId";

        return await conn.ExecuteScalarAsync<bool>(sql, new { Email = email, ExcludeId = excludeId });
    }

    public async Task<IEnumerable<DoctorDto>> GetExpiredDoctorsAsync()
    {
        using var conn = CreateConnection();

        return await conn.QueryAsync<DoctorDto>(
            "sp_GetExpiredDoctors", commandType: CommandType.StoredProcedure);
    }
}
