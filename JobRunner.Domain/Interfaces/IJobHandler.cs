namespace JobRunner.Domain.Interfaces;

public interface IJobHandler
{
    string JobType { get; }

    Task ExecuteAsync(Guid runId, CancellationToken ct);
}
