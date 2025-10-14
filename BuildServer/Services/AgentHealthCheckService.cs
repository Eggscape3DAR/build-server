using Microsoft.EntityFrameworkCore;
using BuildServer.Data;

namespace BuildServer.Services;

public class AgentHealthCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentHealthCheckService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _agentTimeout = TimeSpan.FromSeconds(30);

    public AgentHealthCheckService(
        IServiceProvider serviceProvider,
        ILogger<AgentHealthCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Agent Health Check Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAgentHealth(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking agent health");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Agent Health Check Service stopped");
    }

    private async Task CheckAgentHealth(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BuildServerContext>();

        var cutoffTime = DateTime.UtcNow.Subtract(_agentTimeout);

        // Find all agents that are marked online but haven't sent heartbeat recently
        var staleAgents = await db.Agents
            .Where(a => a.IsOnline && a.LastHeartbeat < cutoffTime)
            .ToListAsync(cancellationToken);

        if (staleAgents.Any())
        {
            foreach (var agent in staleAgents)
            {
                _logger.LogWarning(
                    "Agent {AgentId} ({Name}) marked as offline - last heartbeat: {LastHeartbeat}",
                    agent.AgentId, agent.Name, agent.LastHeartbeat);

                agent.IsOnline = false;
                agent.IsAvailable = false;
            }

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked {Count} agents as offline", staleAgents.Count);
        }
    }
}
