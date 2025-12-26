namespace JobRunner.Models.Responses;

public sealed class JobRunListResponse
{
    public IReadOnlyList<JobRunListItemResponse> Items { get; init; }
        = Array.Empty<JobRunListItemResponse>();
}
