namespace BuildServer.Models;

public class AgentConfiguration
{
    public int Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string BuildOutputFolder { get; set; } = string.Empty;
    public int Priority { get; set; } = 10; // Lower number = higher priority (0 is highest)
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
