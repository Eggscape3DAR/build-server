namespace BuildServer.Models;

public class AgentConfiguration
{
    public int Id { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string UnityProjectPath { get; set; } = string.Empty;
    public string GitUsername { get; set; } = string.Empty;
    public string GitToken { get; set; } = string.Empty;  // TODO: Encrypt!
    public string RepositoryUrl { get; set; } = string.Empty;
    public string WorkspacePath { get; set; } = string.Empty;
    public string ArtifactsPath { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
