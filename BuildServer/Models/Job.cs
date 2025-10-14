namespace BuildServer.Models;

public class Job
{
    public int Id { get; set; }
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public float Progress { get; set; }
    public string? AssignedAgentId { get; set; }
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
