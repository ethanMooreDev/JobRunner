using JobRunner.Domain.Enums;

namespace JobRunner.Domain.Entities;

public class JobAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public AttemptStatus Status { get; set; }
    public int AttemptNumber { get; set; }
    public string? Error { get; set; }

    public Guid? JobRunId { get; set; }
    public JobRun? JobRun { get; set; }
}
