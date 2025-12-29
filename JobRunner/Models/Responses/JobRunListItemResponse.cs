namespace JobRunner.Models.Responses;

public sealed class JobRunListItemResponse
{
    public Guid RunId { get; init; }
    public string JobType { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime? NextVisibleUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}
