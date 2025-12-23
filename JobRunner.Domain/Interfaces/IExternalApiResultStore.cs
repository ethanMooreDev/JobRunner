namespace JobRunner.Domain.Interfaces;

public interface IExternalApiResultStore
{
    public Task SaveAsync(
        Guid runId,
        string provider,
        int httpStatusCode,
        string? responseBody,
        string? error,
        DateTime nowUtc,
        CancellationToken ct);
}
