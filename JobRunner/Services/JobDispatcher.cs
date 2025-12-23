using JobRunner.Domain.Interfaces;

namespace JobRunner.Services;

public class JobDispatcher
{
    private Dictionary<string, IJobHandler> _handlersByType = new(StringComparer.OrdinalIgnoreCase);

    public JobDispatcher(IEnumerable<IJobHandler> handlers)
    {
        foreach(var h in handlers)
        {
            if(_handlersByType.ContainsKey(h.JobType))
            {
                throw new InvalidOperationException($"Multiple handlers registered for job type '{h.JobType}'.");
            }

            _handlersByType[h.JobType] = h;
        }
    }

    public Task ExecuteAsync(string jobType, Guid runId, CancellationToken ct)
    {
        if (!_handlersByType.ContainsKey(jobType))
        {
            throw new InvalidOperationException($"No handler found for job type '{jobType}'.");
        }

        return _handlersByType[jobType].ExecuteAsync(runId, ct);
    }
}
