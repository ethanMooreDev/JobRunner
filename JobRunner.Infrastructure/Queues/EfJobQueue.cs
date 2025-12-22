using JobRunner.Domain.Interfaces;
using JobRunner.Domain.Enums;
using JobRunner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
}
