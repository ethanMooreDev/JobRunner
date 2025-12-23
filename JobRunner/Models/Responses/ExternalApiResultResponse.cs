namespace JobRunner.Models.Responses.JobRuns;

public sealed class ExternalApiResultResponse
{
    public Guid ResultId { get; init; }
    public string Provider { get; init; } = default!;
    public int HttpStatusCode { get; init; }
    public string? ResponseBody { get; init; }
    public string? Error { get; init; }
    public string Status { get; init; } = default!;
    public DateTime CreatedAtUtc { get; init; }
}