using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;
using BuildServer.Models;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly BuildServerContext _db;
    private readonly ILogger<JobsController> _logger;

    public JobsController(BuildServerContext db, ILogger<JobsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
    {
        _logger.LogInformation("Creating new job: {Name} - {ProfileName} - {Platform}",
            request.Name, request.ProfileName, request.Platform);

        var job = new Job
        {
            Name = request.Name,
            ProfileName = request.ProfileName,
            Platform = request.Platform,
            Channel = request.Channel,
            GitBranch = request.GitBranch ?? "",
            GitCommitHash = request.GitCommitHash ?? "",
            GitCommitMessage = request.GitCommitMessage ?? "",
            GitCommitAuthor = request.GitCommitAuthor ?? "",
            GitCommitDate = request.GitCommitDate,
            UploadToGoogleDrive = request.UploadToGoogleDrive,
            UploadToChannel = request.UploadToChannel,
            BuildType = request.BuildType,
            AppVersion = request.AppVersion,
            BundleCode = request.BundleCode,
            Status = JobStatus.Queued,
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Job created with ID: {JobId}", job.JobId);

        return Ok(new { jobId = job.JobId, message = "Job created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var jobs = await _db.Jobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [HttpGet("by-jobid/{jobId}")]
    public async Task<IActionResult> GetByJobId(string jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetNextJob([FromQuery] string agentId)
    {
        // Find first queued job
        var job = await _db.Jobs
            .Where(j => j.Status == JobStatus.Queued)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync();

        if (job == null)
            return NotFound(new { message = "No jobs in queue" });

        // Assign to agent
        job.Status = JobStatus.Assigned;
        job.AssignedAgentId = agentId;
        job.StartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} assigned to agent {AgentId}", job.JobId, agentId);

        return Ok(job);
    }

    [HttpPost("{jobId}/progress")]
    public async Task<IActionResult> UpdateProgress(string jobId, [FromBody] UpdateProgressRequest request)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return NotFound();

        job.Status = JobStatus.Running;
        job.Progress = request.Progress;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Progress updated" });
    }

    [HttpPost("{jobId}/complete")]
    public async Task<IActionResult> Complete(string jobId, [FromBody] CompleteJobRequest request)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return NotFound();

        job.Status = JobStatus.Completed;
        job.Progress = 1.0f;
        job.CompletedAt = DateTime.UtcNow;

        // Update agent performance metrics
        if (!string.IsNullOrEmpty(job.AssignedAgentId) && job.StartedAt.HasValue && job.CompletedAt.HasValue)
        {
            var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == job.AssignedAgentId);
            if (agent != null)
            {
                agent.IsAvailable = true;
                agent.CurrentJobId = null;

                // Calculate build duration
                var buildDuration = (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalSeconds;
                agent.LastBuildDurationSeconds = buildDuration;
                agent.TotalBuildsCompleted++;

                // Calculate average of last 5 builds
                var recentBuilds = await _db.Jobs
                    .Where(j => j.AssignedAgentId == job.AssignedAgentId &&
                                j.Status == JobStatus.Completed &&
                                j.StartedAt.HasValue &&
                                j.CompletedAt.HasValue)
                    .OrderByDescending(j => j.CompletedAt)
                    .Take(5)
                    .ToListAsync();

                if (recentBuilds.Any())
                {
                    var avgDuration = recentBuilds
                        .Select(j => (int)(j.CompletedAt!.Value - j.StartedAt!.Value).TotalSeconds)
                        .Average();
                    agent.AverageBuildDurationSeconds = (int)avgDuration;
                }

                _logger.LogInformation("Agent {AgentId} performance updated: Last={LastDuration}s, Avg={AvgDuration}s, Total={Total}",
                    agent.AgentId, agent.LastBuildDurationSeconds, agent.AverageBuildDurationSeconds, agent.TotalBuildsCompleted);
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Job {JobId} completed successfully", jobId);

        return Ok(new { message = "Job completed successfully" });
    }

    [HttpPost("{jobId}/fail")]
    public async Task<IActionResult> Fail(string jobId, [FromBody] FailJobRequest request)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return NotFound();

        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        job.ErrorMessage = request.ErrorMessage;

        // Mark agent as available
        if (!string.IsNullOrEmpty(job.AssignedAgentId))
        {
            var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == job.AssignedAgentId);
            if (agent != null)
            {
                agent.IsAvailable = true;
                agent.CurrentJobId = null;
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogError("Job {JobId} failed: {ErrorMessage}", jobId, request.ErrorMessage);

        return Ok(new { message = "Job marked as failed" });
    }

    [HttpDelete("{jobId}")]
    public async Task<IActionResult> Delete(string jobId)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        if (job == null)
            return NotFound();

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Job deleted" });
    }
}

public record CreateJobRequest(
    string Name,
    string ProfileName,
    string Platform,
    string Channel,
    string? GitBranch,
    string? GitCommitHash,
    string? GitCommitMessage,
    string? GitCommitAuthor,
    DateTime? GitCommitDate,
    bool UploadToGoogleDrive = true,
    bool UploadToChannel = false,
    string BuildType = "LevelBuilder",
    string AppVersion = "1.0.0",
    int BundleCode = 1
);
public record UpdateProgressRequest(float Progress);
public record CompleteJobRequest(string? BuildPath);
public record FailJobRequest(string ErrorMessage);
