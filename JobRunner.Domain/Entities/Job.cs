using JobRunner.Domain.Enums;

namespace JobRunner.Domain.Entities;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    required public String Name { get; set; }
    required public String JobType { get; set; }

    public List<JobRun> JobRuns { get; set; } = new();
}
