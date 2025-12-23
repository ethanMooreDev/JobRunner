using JobRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobRunner.Infrastructure.Handlers;

public class NoOpJobHandler : IJobHandler
{
    public string JobType => "SampleType";

    private readonly ILogger<NoOpJobHandler> _logger;

    public NoOpJobHandler(ILogger<NoOpJobHandler> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid runId, CancellationToken ct)
    {
        await Task.Delay(300, ct);

        _logger.LogInformation("NoOpJobHandler executed for JobRun {JobRunId}", runId);
    }
}
