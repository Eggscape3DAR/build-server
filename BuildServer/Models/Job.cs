namespace BuildServer.Models;

public class Job
{
    public int Id { get; set; }
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;

    // Git information
    public string GitBranch { get; set; } = string.Empty;
    public string GitCommitHash { get; set; } = string.Empty;
    public string GitCommitMessage { get; set; } = string.Empty;
    public string GitCommitAuthor { get; set; } = string.Empty;
    public DateTime? GitCommitDate { get; set; }

    // Build options
    public bool UploadToGoogleDrive { get; set; } = true;
    public bool UploadToChannel { get; set; } = false;
    public string BuildType { get; set; } = "LevelBuilder"; // LevelBuilder or EarlyAccess
    public string AppVersion { get; set; } = "1.0.0";
    public int BundleCode { get; set; } = 1;

    public JobStatus Status { get; set; } = JobStatus.Queued;
    public float Progress { get; set; }
    public string? AssignedAgentId { get; set; }
    public bool AutoAssign { get; set; } = true; // Auto-assign to next available agent
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum JobStatus
{
    Queued,
    Assigned,
    Running,
    Completed,
    Failed,
    Cancelled
}
