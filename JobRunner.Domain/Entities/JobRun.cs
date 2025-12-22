using JobRunner.Domain.Enums;

namespace JobRunner.Domain.Entities;

public class JobRun
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }
    public RunStatus Status { get; set; }
    public Guid? IdempotencyKey { get; set; }
    public DateTime? NextVisibleUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }

    //public byte[]? RowVersion { get; set; }
    public long Version { get; set; }

    public List<JobAttempt> JobAttempts { get; set; } = new();

    public Guid? JobId { get; set; }
    public Job? Job { get; set; }

    public ExternalApiSyncResult? ExternalApiSyncResult { get; set; }
}
