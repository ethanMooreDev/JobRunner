using JobRunner.Infrastructure.Data;
using JobRunner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobRunner.Domain.Enums;
using JobRunner.Models.Requests;

namespace JobRunner.Controllers;


[ApiController]
[Route("api/job-runs")]
public class JobRunsController : Controller
{
        
    private readonly AppDbContext _context;

    public JobRunsController(AppDbContext context)
    {
        _context = context;
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

        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.JobType == jobType, ct);

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

            await _context.Jobs.AddAsync(job);
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

        await _context.JobRuns.AddAsync(jobRun);

        await _context.SaveChangesAsync();

        return Created($"/api/job-runs/{jobRun.Id}", jobRun.Id.ToString());
    }
}
