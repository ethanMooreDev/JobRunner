namespace JobRunner.Domain.Interfaces;

public interface IJobQueue
{
    public Task<Guid?> TryDequeueAsync(DateTime nowUtc, CancellationToken ct);

    public Task<bool> TryAcquireAsync(Guid runId, DateTime nowUtc, CancellationToken ct);
}
