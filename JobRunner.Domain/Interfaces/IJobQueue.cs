namespace JobRunner.Domain.Interfaces;

public interface IJobQueue
{
    public Task<Guid?> TryDequeueAsync(DateTime nowUtc, CancellationToken ct);

    public Task<bool> TryAcquireAsync(Guid runId, DateTime nowUtc, CancellationToken ct);

    public Task<Guid> CreateAttemptAsync(Guid runId, DateTime nowUtc, CancellationToken ct);

    public Task MarkSucceededAsync(Guid runId, Guid attemptId, DateTime nowUtc, CancellationToken ct);

    public Task MarkFailedAsync(Guid runId, Guid attemptId, DateTime nowUtc, string error, DateTime? nextVisibleUtc, bool terminal, CancellationToken ct);
}
