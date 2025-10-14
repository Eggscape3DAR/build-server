using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly BuildServerContext _db;
    private readonly ILogger<VersionController> _logger;

    public VersionController(BuildServerContext db, ILogger<VersionController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Get the latest app version from completed builds
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestVersion([FromQuery] string? platform = null)
    {
        try
        {
            var query = _db.Jobs
                .Where(j => j.Status == Models.JobStatus.Completed && !string.IsNullOrEmpty(j.AppVersion));

            if (!string.IsNullOrEmpty(platform))
            {
                query = query.Where(j => j.Platform == platform);
            }

            var latestJob = await query
                .OrderByDescending(j => j.CompletedAt)
                .FirstOrDefaultAsync();

            if (latestJob == null)
            {
                // No builds yet, return default
                return Ok(new
                {
                    version = "1.0.0",
                    bundleCode = 1,
                    isDefault = true
                });
            }

            return Ok(new
            {
                version = latestJob.AppVersion,
                bundleCode = latestJob.BundleCode,
                isDefault = false,
                lastBuildDate = latestJob.CompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get latest version");
            return StatusCode(500, new { error = "Failed to retrieve version" });
        }
    }

    /// <summary>
    /// Get the next app version (incremented)
    /// </summary>
    [HttpGet("next")]
    public async Task<IActionResult> GetNextVersion([FromQuery] string? platform = null)
    {
        try
        {
            var latest = await GetLatestVersionInternal(platform);

            // Increment bundle code
            var nextBundleCode = latest.bundleCode + 1;

            // Parse and increment version
            var nextVersion = IncrementVersion(latest.version);

            return Ok(new
            {
                version = nextVersion,
                bundleCode = nextBundleCode,
                previousVersion = latest.version,
                previousBundleCode = latest.bundleCode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next version");
            return StatusCode(500, new { error = "Failed to calculate next version" });
        }
    }

    private async Task<(string version, int bundleCode)> GetLatestVersionInternal(string? platform)
    {
        var query = _db.Jobs
            .Where(j => j.Status == Models.JobStatus.Completed && !string.IsNullOrEmpty(j.AppVersion));

        if (!string.IsNullOrEmpty(platform))
        {
            query = query.Where(j => j.Platform == platform);
        }

        var latestJob = await query
            .OrderByDescending(j => j.CompletedAt)
            .FirstOrDefaultAsync();

        if (latestJob == null)
        {
            return ("1.0.0", 1);
        }

        return (latestJob.AppVersion, latestJob.BundleCode);
    }

    private string IncrementVersion(string version)
    {
        try
        {
            var parts = version.Split('.');
            if (parts.Length != 3)
            {
                return "1.0.1"; // Default increment
            }

            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patch = int.Parse(parts[2]);

            // Increment patch version
            patch++;

            return $"{major}.{minor}.{patch}";
        }
        catch
        {
            return "1.0.1";
        }
    }
}
