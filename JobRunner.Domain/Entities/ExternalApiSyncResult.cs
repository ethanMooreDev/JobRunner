using JobRunner.Domain.Enums;

namespace JobRunner.Domain.Entities;

public class ExternalApiSyncResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public AttemptStatus Status { get; set; }

    public Guid? JobRunId { get; set; }
    public JobRun? JobRun { get; set; }

    public string Provider { get; set; } = "";
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? Error { get; set; }
}
