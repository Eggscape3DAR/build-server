using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;
using BuildServer.Models;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly BuildServerContext _db;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(BuildServerContext db, ILogger<ConfigController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("{agentId}")]
    public async Task<IActionResult> GetConfig(string agentId)
    {
        var config = await _db.AgentConfigurations
            .FirstOrDefaultAsync(c => c.AgentId == agentId);

        if (config == null)
        {
            return NotFound(new { message = "Configuration not found for this agent" });
        }

        return Ok(config);
    }

    [HttpPost("{agentId}")]
    public async Task<IActionResult> SaveConfig(string agentId, [FromBody] SaveConfigRequest request)
    {
        _logger.LogInformation("Saving configuration for agent: {AgentId}", agentId);

        var config = await _db.AgentConfigurations
            .FirstOrDefaultAsync(c => c.AgentId == agentId);

        if (config == null)
        {
            config = new AgentConfiguration
            {
                AgentId = agentId
            };
            _db.AgentConfigurations.Add(config);
        }

        config.BuildOutputFolder = request.BuildOutputFolder ?? config.BuildOutputFolder;
        config.Priority = request.Priority ?? config.Priority;
        config.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Configuration saved for agent: {AgentId}", agentId);

        return Ok(new { message = "Configuration saved successfully" });
    }

    [HttpPut("{agentId}")]
    public async Task<IActionResult> UpdateConfig(string agentId, [FromBody] UpdateConfigRequest request)
    {
        _logger.LogInformation("Updating configuration for agent: {AgentId}", agentId);

        var config = await _db.AgentConfigurations
            .FirstOrDefaultAsync(c => c.AgentId == agentId);

        if (config == null)
        {
            return NotFound(new { message = "Configuration not found" });
        }

        config.BuildOutputFolder = request.BuildOutputFolder;
        config.Priority = request.Priority;
        config.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Configuration updated successfully" });
    }

    [HttpDelete("{agentId}")]
    public async Task<IActionResult> DeleteConfig(string agentId)
    {
        var config = await _db.AgentConfigurations
            .FirstOrDefaultAsync(c => c.AgentId == agentId);

        if (config == null)
            return NotFound();

        _db.AgentConfigurations.Remove(config);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Configuration deleted" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllConfigs()
    {
        var configs = await _db.AgentConfigurations.ToListAsync();
        return Ok(configs);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetGlobalSettings()
    {
        var settings = await _db.GlobalSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            return NotFound(new { message = "Global settings not configured" });
        }

        // Return only necessary fields (don't expose GitHub token)
        return Ok(new
        {
            GoogleDriveFolderId = settings.GoogleDriveFolderId,
            GoogleDriveCredentialsJson = settings.GoogleDriveCredentialsJson,
            RepositoryUrl = settings.RepositoryUrl,
            DefaultBranch = settings.DefaultBranch
        });
    }
}

public record SaveConfigRequest(
    string? BuildOutputFolder,
    int? Priority
);

public record UpdateConfigRequest(
    string BuildOutputFolder,
    int Priority
);
