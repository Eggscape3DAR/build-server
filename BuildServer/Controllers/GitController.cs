using Microsoft.AspNetCore.Mvc;
using BuildServer.Services;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GitController : ControllerBase
{
    private readonly GitService _gitService;
    private readonly ILogger<GitController> _logger;

    public GitController(GitService gitService, ILogger<GitController> logger)
    {
        _gitService = gitService;
        _logger = logger;
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches()
    {
        try
        {
            var branches = await _gitService.GetBranches();
            return Ok(branches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branches");
            return StatusCode(500, new { message = "Error fetching branches", error = ex.Message });
        }
    }

    [HttpGet("commits")]
    public async Task<IActionResult> GetCommits([FromQuery] string branch, [FromQuery] int count = 20)
    {
        try
        {
            if (string.IsNullOrEmpty(branch))
                return BadRequest(new { message = "Branch parameter is required" });

            var commits = await _gitService.GetCommits(branch, count);
            return Ok(commits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commits for branch {Branch}", branch);
            return StatusCode(500, new { message = "Error fetching commits", error = ex.Message });
        }
    }

    [HttpGet("commits/{sha}")]
    public async Task<IActionResult> GetCommit(string sha)
    {
        try
        {
            var commit = await _gitService.GetCommit(sha);
            if (commit == null)
                return NotFound(new { message = "Commit not found" });

            return Ok(commit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commit {Sha}", sha);
            return StatusCode(500, new { message = "Error fetching commit", error = ex.Message });
        }
    }
}
