using LicenseManagementAPI.Application.Interfaces;

namespace LicenseManagementAPI.Infrastructure.Jobs;

/// <summary>
/// Background service that runs once per day at UTC midnight.
/// Marks every Active doctor whose LicenseExpiryDate is in the past as Expired.
///
/// Hybrid scheduling:
///   - On startup it reads the JobLog table to find the last successful run.
///   - If the job has already completed today it skips execution and waits
///     until the next midnight — preventing double-runs on service restarts.
///   - If it has not yet run today it executes immediately (catch-up), then
///     sleeps until the following midnight.
/// </summary>
public sealed class DoctorExpiryHostedService : BackgroundService
{
    private const string JobName = "DoctorExpiryJob";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DoctorExpiryHostedService> _logger;

    public DoctorExpiryHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<DoctorExpiryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogInformation("{Job} started.", JobName);

        // Startup: run immediately if the job has not yet executed today.
        await RunIfDueAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeUntilNextMidnightUtc();

            //_logger.LogInformation(
            //    "{Job}: next scheduled run in {Hours:F1} h (UTC midnight).",
            //    JobName, delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunIfDueAsync(stoppingToken);
        }

        //_logger.LogInformation("{Job} stopped.", JobName);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task RunIfDueAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;

        // Create a DI scope — the service is a singleton but its dependencies
        using var scope = _scopeFactory.CreateScope();
        var jobLogRepo = scope.ServiceProvider.GetRequiredService<IJobLogRepository>();
        var doctorRepo = scope.ServiceProvider.GetRequiredService<IDoctorRepository>();

        try
        {
            var lastRun = await jobLogRepo.GetLastSuccessfulRunAsync(JobName);

            if (lastRun.HasValue && lastRun.Value.Date == DateTime.UtcNow.Date)
            {
                return;
            }

            await ExecuteJobAsync(jobLogRepo, doctorRepo);
        }
        catch (Exception ex)
        {
            // Log but do not rethrow — an unhandled exception here would stop
            // the hosted service entirely. The next iteration will retry.
            _logger.LogError(ex, "{Job}: unexpected error in RunIfDueAsync.", JobName);
        }
    }

    private async Task ExecuteJobAsync(IJobLogRepository jobLogRepo, IDoctorRepository doctorRepo)
    {
        //_logger.LogInformation("{Job}: executing.", JobName);

        var logId = await jobLogRepo.StartAsync(JobName);

        try
        {
            var affected = await doctorRepo.MarkExpiredAsync();

            await jobLogRepo.CompleteAsync(logId, affected);

            //_logger.LogInformation(
            //    "{Job}: completed — {Count} doctor(s) marked Expired.",
            //    JobName, affected);
        }
        catch (Exception ex)
        {
            await jobLogRepo.FailAsync(logId, ex.Message);
            _logger.LogError(ex, "{Job}: failed.", JobName);
            throw;
        }
    }

    private static TimeSpan TimeUntilNextMidnightUtc()
    {
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1); // 00:00:00 of tomorrow (UTC)
        return nextMidnight - now;
    }
}
