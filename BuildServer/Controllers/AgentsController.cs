using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;
using BuildServer.Models;

namespace BuildServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private readonly BuildServerContext _db;
    private readonly ILogger<AgentsController> _logger;

    public AgentsController(BuildServerContext db, ILogger<AgentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
        _logger.LogInformation("Agent registration request: {AgentId} - {Name} - {MachineName}",
            request.AgentId, request.Name, request.MachineName);

        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == request.AgentId);

        if (agent == null)
        {
            agent = new Agent
            {
                AgentId = request.AgentId,
                Name = request.Name,
                MachineName = request.MachineName
            };
            _db.Agents.Add(agent);
            _logger.LogInformation("New agent registered: {AgentId}", request.AgentId);
        }
        else
        {
            _logger.LogInformation("Existing agent re-registered: {AgentId}", request.AgentId);
        }

        agent.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        agent.IsOnline = true;
        agent.LastHeartbeat = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { agentId = agent.AgentId, message = "Registered successfully" });
    }

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatRequest request)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == request.AgentId);
        if (agent == null)
        {
            _logger.LogWarning("Heartbeat from unknown agent: {AgentId}", request.AgentId);
            return NotFound(new { message = "Agent not found. Please register first." });
        }

        agent.LastHeartbeat = DateTime.UtcNow;
        agent.IsOnline = true;
        agent.IsAvailable = request.IsAvailable;
        agent.CurrentJobId = request.CurrentJobId;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Heartbeat received" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _db.Agents
            .OrderByDescending(a => a.LastHeartbeat)
            .ToListAsync();

        return Ok(agents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent == null)
            return NotFound();

        return Ok(agent);
    }

    [HttpGet("by-agentid/{agentId}")]
    public async Task<IActionResult> GetByAgentId(string agentId)
    {
        var agent = await _db.Agents.FirstOrDefaultAsync(a => a.AgentId == agentId);
        if (agent == null)
            return NotFound();

        return Ok(agent);
    }
}

public record RegisterAgentRequest(string AgentId, string Name, string MachineName);
public record HeartbeatRequest(string AgentId, bool IsAvailable, string? CurrentJobId);
