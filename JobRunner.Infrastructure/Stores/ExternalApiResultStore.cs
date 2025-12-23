using JobRunner.Domain.Entities;
using JobRunner.Domain.Enums;
using JobRunner.Domain.Interfaces;
using JobRunner.Infrastructure.Data;

namespace JobRunner.Infrastructure.Stores;

public class ExternalApiResultStore : IExternalApiResultStore
{
    private AppDbContext _db;

    public ExternalApiResultStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(
        Guid runId,
        string provider,
        int httpStatusCode,
        string? responseBody,
        string? error,
        DateTime nowUtc,
        CancellationToken ct)
    {

        var status = error is null && httpStatusCode is >= 200 and < 300
            ? AttemptStatus.Succeeded
            : AttemptStatus.Failed;

        var newResult = new ExternalApiSyncResult {
            Id = Guid.NewGuid(),
            CreatedAtUtc = nowUtc,
            Provider = provider,
            HttpStatusCode = httpStatusCode,
            ResponseBody = responseBody,
            Error = error,
            Status = status,
            JobRunId = runId
        };

        await _db.ExternalApiSyncResults.AddAsync(newResult, ct);
        await _db.SaveChangesAsync(ct);
    }
}
