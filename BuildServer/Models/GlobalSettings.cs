namespace BuildServer.Models;

public class GlobalSettings
{
    public int Id { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string RepositoryOwner { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string GitHubToken { get; set; } = string.Empty; // TODO: Encrypt!
    public string DefaultBranch { get; set; } = "development";

    // Portal Server settings
    public string PortalServerUrl { get; set; } = "https://eggscapeportalserver.onrender.com";
    public string PortalServerSecret { get; set; } = "bWF0aSB0aWVuZSBtdWNob3MgaHVldm9zIGVuIGVsIGN1bG8=";

    // Google Drive settings
    public string GoogleDriveFolderId { get; set; } = "127SFVubjMJrQfS965hm5Cl3V6h4yfcru";
    public string GoogleDriveCredentialsJson { get; set; } = string.Empty; // Service account JSON

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
