using Dapper;
using LicenseManagementAPI.Application.Interfaces;

namespace LicenseManagementAPI.Infrastructure.Repositories;

public class JobLogRepository : IJobLogRepository
{
    private readonly IDbConnectionFactory _db;

    public JobLogRepository(IDbConnectionFactory db) => _db = db;

    public async Task<DateTime?> GetLastSuccessfulRunAsync(string jobName)
    {
        using var conn = _db.Create();

        return await conn.ExecuteScalarAsync<DateTime?>("""
            SELECT TOP 1 CompletedAt
            FROM   dbo.JobLog
            WHERE  JobName  = @JobName
              AND  [Status] = 'Completed'
            ORDER  BY CompletedAt DESC
            """, new { JobName = jobName });
    }

    public async Task<int> StartAsync(string jobName)
    {
        using var conn = _db.Create();

        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO dbo.JobLog (JobName, StartedAt, [Status])
            OUTPUT INSERTED.Id
            VALUES (@JobName, @StartedAt, 'Running')
            """, new { JobName = jobName, StartedAt = DateTime.UtcNow });
    }

    public async Task CompleteAsync(int id, int recordsAffected)
    {
        using var conn = _db.Create();

        await conn.ExecuteAsync("""
            UPDATE dbo.JobLog
            SET    [Status]         = 'Completed',
                   CompletedAt      = @CompletedAt,
                   RecordsAffected  = @RecordsAffected
            WHERE  Id = @Id
            """, new { Id = id, CompletedAt = DateTime.UtcNow, RecordsAffected = recordsAffected });
    }

    public async Task FailAsync(int id, string errorMessage)
    {
        using var conn = _db.Create();

        await conn.ExecuteAsync("""
            UPDATE dbo.JobLog
            SET    [Status]      = 'Failed',
                   CompletedAt   = @CompletedAt,
                   ErrorMessage  = @ErrorMessage
            WHERE  Id = @Id
            """, new { Id = id, CompletedAt = DateTime.UtcNow, ErrorMessage = errorMessage });
    }
}
