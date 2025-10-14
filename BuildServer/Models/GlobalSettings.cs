namespace BuildServer.Models;

public class GlobalSettings
{
    public int Id { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string RepositoryOwner { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string GitHubToken { get; set; } = string.Empty; // TODO: Encrypt!
    public string DefaultBranch { get; set; } = "development";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
