namespace JobRunner.Domain.Enums;

public enum RunStatus
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Canceled
}
