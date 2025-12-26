using JobRunner.Domain.Entities;
using JobRunner.Domain.Enums;
using JobRunner.Infrastructure.Data;
using JobRunner.Models.Requests;
using JobRunner.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRunner.Controllers;


[ApiController]
[Route("api/job-runs")]
public class JobRunsController : Controller
{
        
    private readonly AppDbContext _db;

    public JobRunsController(AppDbContext context)
    {
        _db = context;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobRunDetailsResponse>> GetById(Guid id, CancellationToken ct)
    {
        var response = await QueryJobRunDetailsAsync(id, ct);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(JobRunListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<JobRunListResponse>> List(
       [FromQuery] string? status,
       [FromQuery] int limit = 50,
       CancellationToken ct = default)
    {
        if (limit <= 0) limit = 50;
        if (limit > 200) limit = 200;

        var statusFilter = TryParseRunStatus(status);

        var query = _db.JobRuns
            .AsNoTracking();

        if (statusFilter is not null)
        {
            query = query.Where(r => r.Status == statusFilter.Value);
        }

        query = statusFilter == RunStatus.Queued
            ? query.OrderBy(r => r.NextVisibleUtc).ThenBy(r => r.CreatedAtUtc)
            : query.OrderByDescending(r => r.CreatedAtUtc);

        var items = await query
            .Take(limit)
            .Select(r => new JobRunListItemResponse
            {
                RunId = r.Id,
                JobType = r.Job != null ? r.Job.JobType : "",
                Status = r.Status.ToString(),
                NextVisibleUtc = r.NextVisibleUtc ?? r.CreatedAtUtc,
                StartedAtUtc = r.StartedAtUtc,
                CompletedAtUtc = r.CompletedAtUtc
            })
            .ToListAsync(ct);

        return Ok(new JobRunListResponse { Items = items });
    }

    [HttpPost("enqueue")]
    public async Task<ActionResult<string>> Enqueue([FromBody] EnqueueRequest request, CancellationToken ct)
    {

        if(string.IsNullOrWhiteSpace(request.JobType))
        {
            return BadRequest("JobType is required.");
        }

        var jobType = request.JobType.Trim();

        var nowUtc = DateTime.UtcNow;

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobType == jobType, ct);

        if (job is null)
        {
            job = new Job
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = nowUtc,
                Name = jobType,
                JobType = jobType,
                MaxAttempts = 3,
                JobRuns = new List<JobRun>()
            };

            await _db.Jobs.AddAsync(job);
        }

        var jobRun = new JobRun
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTime.UtcNow,
            Status = RunStatus.Queued,
            JobId = job.Id,
            JobAttempts = new List<JobAttempt>(),
            NextVisibleUtc = nowUtc.AddSeconds(5)
        };

        await _db.JobRuns.AddAsync(jobRun);

        await _db.SaveChangesAsync();

        return Created($"/api/job-runs/{jobRun.Id}", jobRun.Id.ToString());
    }

    private async Task<JobRunDetailsResponse?> QueryJobRunDetailsAsync(Guid id, CancellationToken ct)
    {
        return await _db.JobRuns
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new JobRunDetailsResponse
            {
                RunId = r.Id,
                JobType = r.Job != null ? r.Job.JobType : "",
                Status = r.Status.ToString(),
                NextVisibleUtc = r.NextVisibleUtc ?? r.CreatedAtUtc,
                StartedAtUtc = r.StartedAtUtc,
                CompletedAtUtc = r.CompletedAtUtc,

                Attempts = r.JobAttempts
                    .OrderBy(a => a.AttemptNumber)
                    .Select(a => new JobAttemptResponse
                    {
                        AttemptId = a.Id,
                        AttemptNumber = a.AttemptNumber,
                        Status = a.Status.ToString(),
                        Error = a.Error,
                        StartedAtUtc = a.CreatedAtUtc, 
                        CompletedAtUtc = a.CompletedAtUtc
                    })
                    .ToList(),

                LatestResult = r.ExternalApiSyncResult == null
                    ? null
                    : new ExternalApiResultResponse
                    {
                        ResultId = r.ExternalApiSyncResult.Id,
                        Provider = r.ExternalApiSyncResult.Provider,
                        HttpStatusCode = r.ExternalApiSyncResult.HttpStatusCode,
                        ResponseBody = r.ExternalApiSyncResult.ResponseBody,
                        Error = r.ExternalApiSyncResult.Error,
                        Status = r.ExternalApiSyncResult.Status.ToString(),
                        CreatedAtUtc = r.ExternalApiSyncResult.CreatedAtUtc
                    }
            })
            .SingleOrDefaultAsync(ct);
    }

    private static RunStatus? TryParseRunStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return Enum.TryParse<RunStatus>(status, ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }

}
