using JobRunner.Domain.Interfaces;
using JobRunner.Infrastructure.Data;
using JobRunner.Services;
using Microsoft.EntityFrameworkCore;

namespace JobRunner.Workers;

public class JobRunnerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobRunnerWorker> _logger;

    public JobRunnerWorker(IServiceScopeFactory scopeFactory, ILogger<JobRunnerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                using var scope = _scopeFactory.CreateScope();
                var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue>();

                var runId = await jobQueue.TryDequeueAsync(now, stoppingToken);

                if (runId != null)
                {
                    var acquired = await jobQueue.TryAcquireAsync(runId.Value, now, stoppingToken);

                    if (acquired)
                    {
                        _logger.LogInformation("Acquired job run {JobRunId} for processing.", runId.Value);

                        var attemptStart = await jobQueue.CreateAttemptAsync(runId.Value, DateTime.UtcNow, stoppingToken);

                        var attemptId = attemptStart.AttemptId;
                        var attemptNumber = attemptStart.AttemptNumber;
                        var maxAttempts = attemptStart.MaxAttempts;
                        string? jobType = null;

                        _logger.LogInformation("Attempt {AttemptNumber}/{MaxAttempts}", attemptNumber, maxAttempts);

                        try
                        {
                            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                            var runIdValue = runId.Value;

                            jobType = await db.JobRuns
                                .AsNoTracking()
                                .Where(r => r.Id == runIdValue)
                                .Select(r => r.Job!.JobType)
                                .SingleAsync(stoppingToken);

                            _logger.LogInformation(
                                "Job succeeded. RunId={RunId} AttemptId={AttemptId} JobType={JobType}",
                                runId, attemptId, jobType);

                            var dispatcher = scope.ServiceProvider.GetRequiredService<JobDispatcher>();

                            await dispatcher.ExecuteAsync(jobType, runIdValue, stoppingToken);

                            await jobQueue.MarkSucceededAsync(runIdValue, attemptId, DateTime.UtcNow, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "Job failed. RunId={RunId} AttemptId={AttemptId} JobType={JobType}",
                                runId, attemptId, jobType ?? "<unknown>");

                            var failedAtUtc = DateTime.UtcNow;
                            var terminal = IsPermanent(ex) || attemptNumber >= maxAttempts;

                            if (!terminal)
                            {
                                var backOff = GetBackOff(attemptNumber);
                                _logger.LogInformation("Scheduling retry for job run {JobRunId} in {BackOff} seconds.", runId.Value, backOff.TotalSeconds);
                                await jobQueue.MarkFailedAsync(runId.Value, attemptId, failedAtUtc, SanitizeError(ex), failedAtUtc.Add(backOff), terminal, stoppingToken);
                                continue;
                            }

                            await jobQueue.MarkFailedAsync(runId.Value, attemptId, failedAtUtc, SanitizeError(ex), null, terminal, stoppingToken);
                            continue;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Failed to acquire job run {JobRunId}, it may have been taken by another worker.", runId.Value);
                        await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken);
                    }
                }
                else
                {
                    // No job run available, wait before checking again
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the JobRunnerWorker.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    private static TimeSpan GetBackOff(int attemptNumber)
    {
        return TimeSpan.FromSeconds(Math.Min(300, 5 * Math.Pow(2, attemptNumber - 1)));
    }

    private static bool IsPermanent(Exception ex)
    {
        return ex is InvalidOperationException;
    }

    private static string SanitizeError(Exception ex)
    {
        return $"{ex.GetType().Name}: {ex.Message}";
    }


}
