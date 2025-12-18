using JobRunner.Domain.Enums;

namespace JobRunner.Domain.Entities;

public class ExternalApiSyncResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public AttemptStatus Status { get; set; }

    public Guid? JobRunId { get; set; }
    public JobRun? JobRun { get; set; }
}
