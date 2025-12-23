namespace JobRunner.Models.Responses;

public sealed class JobAttemptResponse
{
    public Guid AttemptId { get; init; }
    public int AttemptNumber { get; init; }
    public string Status { get; init; } = default!;
    public string? Error { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
