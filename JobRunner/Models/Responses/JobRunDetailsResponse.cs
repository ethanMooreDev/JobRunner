namespace JobRunner.Models.Responses;

public sealed class JobRunDetailsResponse
{
    public Guid RunId { get; init; }
    public string JobType { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime? NextVisibleUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }

    public IReadOnlyList<JobAttemptResponse> Attempts { get; init; } = Array.Empty<JobAttemptResponse>();

    // Nullable because queued runs won't have a result yet.
    public ExternalApiResultResponse? LatestResult { get; init; }
}
