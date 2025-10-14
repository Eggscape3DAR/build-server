using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;
using BuildServer.Models;
using System.Web;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly BuildServerContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VersionController> _logger;

    public VersionController(BuildServerContext db, HttpClient httpClient, ILogger<VersionController> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
    }

    private string GetChannelId(string buildType)
    {
        return buildType switch
        {
            "EarlyAccess" => "com.ThreeDar.eggscape_early_access",
            "LevelBuilder" => "com.ThreeDar.eggscape_early_dev",
            _ => "com.ThreeDar.eggscape_early_dev"
        };
    }

    /// <summary>
    /// Get the latest bundle code from Portal Server
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestVersion([FromQuery] string buildType = "LevelBuilder")
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return StatusCode(500, new { error = "Global settings not configured" });
            }

            var channelId = GetChannelId(buildType);
            var url = $"{settings.PortalServerUrl}/bcv?v={HttpUtility.UrlEncode(channelId)}";

            _logger.LogInformation("Fetching bundle code from: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var bundleCodeStr = await response.Content.ReadAsStringAsync();

            if (!int.TryParse(bundleCodeStr.Trim(), out var bundleCode))
            {
                _logger.LogError("Invalid bundle code received: {BundleCode}", bundleCodeStr);
                return StatusCode(500, new { error = "Invalid bundle code from portal server" });
            }

            return Ok(new
            {
                version = "1.0.0", // Version will be managed separately
                bundleCode = bundleCode,
                channelId = channelId,
                buildType = buildType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get bundle code from portal server");
            return StatusCode(500, new { error = $"Failed to retrieve bundle code: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get the next bundle code (current + 1)
    /// </summary>
    [HttpGet("next")]
    public async Task<IActionResult> GetNextVersion([FromQuery] string buildType = "LevelBuilder")
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return StatusCode(500, new { error = "Global settings not configured" });
            }

            var channelId = GetChannelId(buildType);
            var url = $"{settings.PortalServerUrl}/bcv?v={HttpUtility.UrlEncode(channelId)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var bundleCodeStr = await response.Content.ReadAsStringAsync();

            if (!int.TryParse(bundleCodeStr.Trim(), out var currentBundleCode))
            {
                _logger.LogError("Invalid bundle code received: {BundleCode}", bundleCodeStr);
                return StatusCode(500, new { error = "Invalid bundle code from portal server" });
            }

            var nextBundleCode = currentBundleCode + 1;

            return Ok(new
            {
                version = "1.0.0", // Placeholder for now
                bundleCode = nextBundleCode,
                previousBundleCode = currentBundleCode,
                channelId = channelId,
                buildType = buildType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next bundle code");
            return StatusCode(500, new { error = $"Failed to calculate next bundle code: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update bundle code in Portal Server
    /// </summary>
    [HttpPost("update-bundle-code")]
    public async Task<IActionResult> UpdateBundleCode([FromBody] UpdateBundleCodeRequest request)
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return StatusCode(500, new { error = "Global settings not configured" });
            }

            var channelId = GetChannelId(request.BuildType);

            var url = $"{settings.PortalServerUrl}/bcv?" +
                      $"v={HttpUtility.UrlEncode(channelId)}&" +
                      $"s={HttpUtility.UrlEncode(settings.PortalServerSecret)}&" +
                      $"bc={request.BundleCode}";

            _logger.LogInformation("Updating bundle code to {BundleCode} for {ChannelId}", request.BundleCode, channelId);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Bundle code updated successfully: {Result}", result);

            return Ok(new
            {
                success = true,
                bundleCode = request.BundleCode,
                channelId = channelId,
                buildType = request.BuildType,
                message = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update bundle code");
            return StatusCode(500, new { error = $"Failed to update bundle code: {ex.Message}" });
        }
    }
}

public record UpdateBundleCodeRequest(string BuildType, int BundleCode);
