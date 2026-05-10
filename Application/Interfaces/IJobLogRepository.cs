namespace LicenseManagementAPI.Application.Interfaces;

public interface IJobLogRepository
{
    /// <summary>Returns the CompletedAt timestamp of the most recent successful run, or null if never run.</summary>
    Task<DateTime?> GetLastSuccessfulRunAsync(string jobName);

    /// <summary>Inserts a Running entry and returns its Id.</summary>
    Task<int> StartAsync(string jobName);

    /// <summary>Marks an entry as Completed.</summary>
    Task CompleteAsync(int id, int recordsAffected);

    /// <summary>Marks an entry as Failed.</summary>
    Task FailAsync(int id, string errorMessage);
}
