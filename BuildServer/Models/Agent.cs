namespace BuildServer.Models;

public class Agent
{
    public int Id { get; set; }
    public string AgentId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public string? CurrentJobId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Performance metrics
    public int? LastBuildDurationSeconds { get; set; }
    public int? AverageBuildDurationSeconds { get; set; }
    public int TotalBuildsCompleted { get; set; }
}
