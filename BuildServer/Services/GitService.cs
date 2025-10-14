using Octokit;
using BuildServer.Data;
using Microsoft.EntityFrameworkCore;

namespace BuildServer.Services;

public class GitService
{
    private readonly BuildServerContext _db;
    private readonly ILogger<GitService> _logger;

    public GitService(BuildServerContext db, ILogger<GitService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private async Task<GitHubClient?> GetGitHubClient()
    {
        var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
        if (settings == null || string.IsNullOrEmpty(settings.GitHubToken))
        {
            _logger.LogWarning("No GitHub credentials configured");
            return null;
        }

        var client = new GitHubClient(new ProductHeaderValue("EggscapeBuildServer"));
        client.Credentials = new Credentials(settings.GitHubToken);
        return client;
    }

    public async Task<List<BranchInfo>> GetBranches()
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                _logger.LogWarning("No global settings configured");
                return new List<BranchInfo>();
            }

            var client = await GetGitHubClient();
            if (client == null)
                return new List<BranchInfo>();

            var branches = await client.Repository.Branch.GetAll(
                settings.RepositoryOwner,
                settings.RepositoryName);

            return branches.Select(b => new BranchInfo
            {
                Name = b.Name,
                CommitSha = b.Commit.Sha,
                CommitUrl = b.Commit.Url
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching branches from GitHub");
            return new List<BranchInfo>();
        }
    }

    public async Task<List<CommitInfo>> GetCommits(string branch, int count = 20)
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                _logger.LogWarning("No global settings configured");
                return new List<CommitInfo>();
            }

            var client = await GetGitHubClient();
            if (client == null)
                return new List<CommitInfo>();

            var request = new CommitRequest
            {
                Sha = branch
            };

            var commits = await client.Repository.Commit.GetAll(
                settings.RepositoryOwner,
                settings.RepositoryName,
                request,
                new ApiOptions { PageSize = count, PageCount = 1 });

            return commits.Select(c => new CommitInfo
            {
                Sha = c.Sha,
                ShortSha = c.Sha.Substring(0, 7),
                Message = c.Commit.Message,
                Author = c.Commit.Author.Name,
                AuthorEmail = c.Commit.Author.Email,
                Date = c.Commit.Author.Date.DateTime,
                Url = c.HtmlUrl
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commits from GitHub for branch {Branch}", branch);
            return new List<CommitInfo>();
        }
    }

    public async Task<CommitInfo?> GetCommit(string sha)
    {
        try
        {
            var settings = await _db.GlobalSettings.FirstOrDefaultAsync();
            if (settings == null)
                return null;

            var client = await GetGitHubClient();
            if (client == null)
                return null;

            var commit = await client.Repository.Commit.Get(
                settings.RepositoryOwner,
                settings.RepositoryName,
                sha);

            return new CommitInfo
            {
                Sha = commit.Sha,
                ShortSha = commit.Sha.Substring(0, 7),
                Message = commit.Commit.Message,
                Author = commit.Commit.Author.Name,
                AuthorEmail = commit.Commit.Author.Email,
                Date = commit.Commit.Author.Date.DateTime,
                Url = commit.HtmlUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching commit {Sha} from GitHub", sha);
            return null;
        }
    }
}

public class BranchInfo
{
    public string Name { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string CommitUrl { get; set; } = string.Empty;
}

public class CommitInfo
{
    public string Sha { get; set; } = string.Empty;
    public string ShortSha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Url { get; set; } = string.Empty;
}
