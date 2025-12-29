using JobRunner.Domain.Interfaces;
using JobRunner.Domain.Enums;
using JobRunner.Domain.Models;
using JobRunner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using JobRunner.Domain.Entities;
using System.Net.WebSockets;

namespace JobRunner.Infrastructure.Queues;

public class EfJobQueue : IJobQueue
{
    private readonly AppDbContext _db;

    public EfJobQueue(AppDbContext appDbContext)
    {
        _db = appDbContext;
    }
    public async Task<Guid?> TryDequeueAsync(DateTime nowUtc, CancellationToken ct)
    {
        return (await _db.JobRuns
            .AsNoTracking()
            .Where(r =>
                r.Status == RunStatus.Queued &&
                (r.NextVisibleUtc == null || r.NextVisibleUtc <= nowUtc)
            )
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(ct));
    }
    public async Task<bool> TryAcquireAsync(Guid runId, DateTime nowUtc, CancellationToken ct)
    {
        var rowsAffected = await _db.JobRuns
            .Where(r =>
                r.Id == runId &&
                r.Status == RunStatus.Queued &&
                (r.NextVisibleUtc == null || r.NextVisibleUtc <= nowUtc)
            )
            .ExecuteUpdateAsync(jobRun =>
            {
                jobRun
                    .SetProperty(s => s.Status, RunStatus.Running)
                    .SetProperty(s => s.StartedAtUtc, nowUtc)
                    .SetProperty(s => s.Version, s => s.Version + 1);
            }, ct);

        return rowsAffected == 1;
    }

    public async Task<AttemptStart> CreateAttemptAsync(Guid runId, DateTime nowUtc, CancellationToken ct)
    {
        var attemptInfo = await _db.JobRuns
            .AsNoTracking()
            .Where(r => r.Id == runId && r.Job != null && r.Status == RunStatus.Running)
            .Select(r => new { r.JobAttempts.Count, r.Job!.MaxAttempts })
            .FirstOrDefaultAsync(ct);

        if (attemptInfo == null)
        {
            throw new InvalidOperationException($"Cannot create attempt for job run {runId} because it does not exist or is not running.");
        }

        if(attemptInfo.MaxAttempts <= 0)
        {
            throw new InvalidOperationException($"Cannot create attempt for job run {runId} because the job allows zero attempts.");
        }

        var attempt = new JobAttempt
        {
            Id = Guid.NewGuid(),
            JobRunId = runId,
            AttemptNumber = attemptInfo.Count + 1,
            Status = AttemptStatus.Running,
            CreatedAtUtc = nowUtc
        };

        _db.JobAttempts.Add(attempt);

        await _db.SaveChangesAsync(ct);

        return new AttemptStart(attempt.Id, attempt.AttemptNumber, attemptInfo.MaxAttempts);
    }
    public async Task MarkSucceededAsync(Guid runId, Guid attemptId, DateTime nowUtc, CancellationToken ct)
    {
        int rowsAffected = await _db.JobRuns
            .Where(r => r.Id == runId && r.Status == RunStatus.Running)
            .ExecuteUpdateAsync(jobRun =>
            {
                jobRun
                    .SetProperty(s => s.Status, RunStatus.Succeeded)
                    .SetProperty(s => s.CompletedAtUtc, nowUtc)
                    .SetProperty(s => s.NextVisibleUtc, (DateTime?) null)
                    .SetProperty(s => s.Version, s => s.Version + 1);
            }, ct);

        if (rowsAffected != 1)
        {
            throw new InvalidOperationException($"Cannot mark job run {runId} as succeeded because it does not exist or is not running.");
        }

        rowsAffected = await _db.JobAttempts
            .Where(a => a.Id == attemptId && a.JobRunId == runId && a.Status == AttemptStatus.Running)
            .ExecuteUpdateAsync(jobAttempt =>
            {
                jobAttempt
                    .SetProperty(s => s.Status, AttemptStatus.Succeeded)
                    .SetProperty(s => s.CompletedAtUtc, nowUtc);
            }, ct);

        if (rowsAffected != 1)
        {
            throw new InvalidOperationException($"Cannot mark job attempt {attemptId} as succeeded because it does not exist or is not running.");
        }
    }
    public async Task MarkFailedAsync(Guid runId, Guid attemptId, DateTime nowUtc, string error, DateTime? nextVisibleUtc, bool terminal, CancellationToken ct)
    {
        if(!terminal && nextVisibleUtc == null)
        {
            throw new ArgumentException("nextVisibleUtc must be provided when terminal is false.", nameof(nextVisibleUtc));
        }

        int rowsAffected = await _db.JobAttempts
            .Where(a => a.Id == attemptId && a.JobRunId == runId && a.Status == AttemptStatus.Running)
            .ExecuteUpdateAsync(jobAttempt =>
            {
                jobAttempt
                    .SetProperty(s => s.Status, AttemptStatus.Failed)
                    .SetProperty(s => s.CompletedAtUtc, nowUtc)
                    .SetProperty(s => s.Error, error);
            }, ct);

        if (rowsAffected != 1)
        {
            throw new InvalidOperationException($"Cannot mark job attempt {attemptId} as failed because it does not exist or is not running.");
        }

        if(terminal)
        {
            rowsAffected = await _db.JobRuns
           .Where(r => r.Id == runId && r.Status == RunStatus.Running)
           .ExecuteUpdateAsync(jobRun =>
           {
               jobRun
                    .SetProperty(s => s.Status, RunStatus.Failed)
                    .SetProperty(s => s.CompletedAtUtc, nowUtc)
                    .SetProperty(s => s.NextVisibleUtc, (DateTime?) null)
                    .SetProperty(s => s.Version, s => s.Version + 1);
           }, ct);
        }
        else
        {
            rowsAffected = await _db.JobRuns
           .Where(r => r.Id == runId && r.Status == RunStatus.Running)
           .ExecuteUpdateAsync(jobRun =>
           {
                jobRun
                    .SetProperty(s => s.Status, RunStatus.Queued)
                    .SetProperty(s => s.NextVisibleUtc, nextVisibleUtc)
                    .SetProperty(s => s.Version, s => s.Version + 1)
                    .SetProperty(s => s.CompletedAtUtc, (DateTime?) null);
           }, ct);
        }

        if(rowsAffected != 1)
        {
            throw new InvalidOperationException($"Cannot mark job run {runId} as failed because it does not exist or is not running.");
        }
    }
}
