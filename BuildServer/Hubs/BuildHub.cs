using Microsoft.AspNetCore.SignalR;

namespace BuildServer.Hubs;

public class BuildHub : Hub
{
    private readonly ILogger<BuildHub> _logger;

    public BuildHub(ILogger<BuildHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // Client can join a specific job group to receive updates
    public async Task JoinJobGroup(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"job-{jobId}");
        _logger.LogInformation("Client {ConnectionId} joined job group: {JobId}", Context.ConnectionId, jobId);
    }

    public async Task LeaveJobGroup(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"job-{jobId}");
        _logger.LogInformation("Client {ConnectionId} left job group: {JobId}", Context.ConnectionId, jobId);
    }

    // Server-side methods to broadcast updates
    public async Task BroadcastAgentUpdate(string agentId, bool isOnline, bool isAvailable)
    {
        await Clients.All.SendAsync("AgentUpdated", new
        {
            agentId,
            isOnline,
            isAvailable,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastJobProgress(string jobId, float progress, string currentStep)
    {
        await Clients.Group($"job-{jobId}").SendAsync("JobProgressUpdated", new
        {
            jobId,
            progress,
            currentStep,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastJobCompleted(string jobId, bool success, string? buildPath)
    {
        await Clients.All.SendAsync("JobCompleted", new
        {
            jobId,
            success,
            buildPath,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastJobCreated(string jobId, string name)
    {
        await Clients.All.SendAsync("JobCreated", new
        {
            jobId,
            name,
            timestamp = DateTime.UtcNow
        });
    }
}
