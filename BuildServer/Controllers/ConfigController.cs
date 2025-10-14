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

        config.UnityProjectPath = request.UnityProjectPath ?? config.UnityProjectPath;
        config.GitUsername = request.GitUsername ?? config.GitUsername;
        config.GitToken = request.GitToken ?? config.GitToken; // TODO: Encrypt!
        config.RepositoryUrl = request.RepositoryUrl ?? config.RepositoryUrl;
        config.WorkspacePath = request.WorkspacePath ?? config.WorkspacePath;
        config.ArtifactsPath = request.ArtifactsPath ?? config.ArtifactsPath;
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

        config.UnityProjectPath = request.UnityProjectPath;
        config.GitUsername = request.GitUsername;
        config.GitToken = request.GitToken; // TODO: Encrypt!
        config.RepositoryUrl = request.RepositoryUrl;
        config.WorkspacePath = request.WorkspacePath;
        config.ArtifactsPath = request.ArtifactsPath;
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
}

public record SaveConfigRequest(
    string? UnityProjectPath,
    string? GitUsername,
    string? GitToken,
    string? RepositoryUrl,
    string? WorkspacePath,
    string? ArtifactsPath
);

public record UpdateConfigRequest(
    string UnityProjectPath,
    string GitUsername,
    string GitToken,
    string RepositoryUrl,
    string WorkspacePath,
    string ArtifactsPath
);
