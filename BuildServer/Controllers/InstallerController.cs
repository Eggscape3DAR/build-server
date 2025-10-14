using Microsoft.AspNetCore.Mvc;

namespace BuildServer.Controllers;

[ApiController]
[Route("[controller]")]
public class InstallerController : ControllerBase
{
    private readonly ILogger<InstallerController> _logger;
    private readonly IWebHostEnvironment _environment;

    public InstallerController(ILogger<InstallerController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("download")]
    public IActionResult DownloadInstaller()
    {
        try
        {
            var installerPath = Path.Combine(_environment.WebRootPath, "installer", "EggscapeBuildAgent_Setup_v1.0.0.exe");

            if (!System.IO.File.Exists(installerPath))
            {
                _logger.LogWarning("Installer file not found at: {Path}", installerPath);
                return NotFound(new { message = "Installer not found. Please contact the administrator." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(installerPath);
            return File(fileBytes, "application/octet-stream", "EggscapeBuildAgent_Setup_v1.0.0.exe");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading installer");
            return StatusCode(500, new { message = "Error downloading installer" });
        }
    }

    [HttpGet("info")]
    public IActionResult GetInstallerInfo()
    {
        try
        {
            var installerPath = Path.Combine(_environment.WebRootPath, "installer", "EggscapeBuildAgent_Setup_v1.0.0.exe");

            if (!System.IO.File.Exists(installerPath))
            {
                return NotFound(new { message = "Installer not found" });
            }

            var fileInfo = new FileInfo(installerPath);
            return Ok(new
            {
                fileName = "EggscapeBuildAgent_Setup_v1.0.0.exe",
                version = "1.0.0",
                sizeBytes = fileInfo.Length,
                sizeMB = Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2),
                lastModified = fileInfo.LastWriteTimeUtc,
                available = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting installer info");
            return StatusCode(500, new { message = "Error getting installer info" });
        }
    }
}
