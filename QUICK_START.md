# 🚀 Quick Start - Build Server

**Get the build server running in 5 minutes**

---

## 📦 What You Have

Un proyecto Blazor Server listo para ser el **centralized build server**.

**Lo clave:** El instalador del agente ahora SOLO pregunta la URL del server. Todo lo demás se configura desde la web!

---

## 🎯 Setup del Repo en GitHub

### 1. Crear repo en GitHub

1. Ve a: https://github.com/Eggscape3DAR/
2. Click "New repository"
3. Nombre: `build-server`
4. Descripción: "Centralized build orchestration server for Eggscape Unity builds"
5. Public o Private (tu elección)
6. **NO** marques "Initialize with README" (ya tenemos uno)
7. Click "Create repository"

### 2. Push el código

```bash
cd C:/build-server

# Inicializar git
git init
git add .
git commit -m "Initial commit - Build Server MVP

- Blazor Server project structure
- SQLite database ready
- REST API endpoints planned
- Web UI pages planned
- Real-time updates via SignalR
- Complete documentation"

# Conectar con GitHub
git remote add origin https://github.com/Eggscape3DAR/build-server.git
git branch -M main
git push -u origin main
```

---

## 🛠️ Implementación Mínima (MVP)

Para tener algo funcionando AHORA, necesitas implementar estos archivos clave:

### 1. Modelos (15 min)

Crea `BuildServer/Models/Agent.cs`:
```csharp
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
}
```

Crea `BuildServer/Models/Job.cs`:
```csharp
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
```

Crea `BuildServer/Models/AgentConfiguration.cs`:
```csharp
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
```

### 2. Database Context (10 min)

Crea `BuildServer/Data/BuildServerContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using BuildServer.Models;

namespace BuildServer.Data;

public class BuildServerContext : DbContext
{
    public BuildServerContext(DbContextOptions<BuildServerContext> options)
        : base(options) { }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<AgentConfiguration> AgentConfigurations => Set<AgentConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>()
            .HasIndex(a => a.AgentId)
            .IsUnique();

        modelBuilder.Entity<Job>()
            .HasIndex(j => j.JobId)
            .IsUnique();

        modelBuilder.Entity<AgentConfiguration>()
            .HasIndex(ac => ac.AgentId)
            .IsUnique();
    }
}
```

### 3. Update Program.cs (5 min)

Reemplaza `BuildServer/Program.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using BuildServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<BuildServerContext>(options =>
    options.UseSqlite("Data Source=buildserver.db"));

// Swagger (API docs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create database if not exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BuildServerContext>();
    db.Database.EnsureCreated();
}

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
```

### 4. Simple API Controller (10 min)

Crea `BuildServer/Controllers/AgentsController.cs`:
```csharp
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

    public AgentsController(BuildServerContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentRequest request)
    {
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
            return NotFound();

        agent.LastHeartbeat = DateTime.UtcNow;
        agent.IsOnline = true;
        agent.IsAvailable = request.IsAvailable;
        agent.CurrentJobId = request.CurrentJobId;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _db.Agents.ToListAsync();
        return Ok(agents);
    }
}

public record RegisterAgentRequest(string AgentId, string Name, string MachineName);
public record HeartbeatRequest(string AgentId, bool IsAvailable, string? CurrentJobId);
```

### 5. Simple Blazor Page (15 min)

Modifica `BuildServer/Pages/Index.razor`:
```razor
@page "/"
@using Microsoft.EntityFrameworkCore
@using BuildServer.Data
@using BuildServer.Models
@inject BuildServerContext DB

<PageTitle>Build Server - Dashboard</PageTitle>

<h1>🚀 Eggscape Build Server</h1>

<div class="row mt-4">
    <div class="col-md-4">
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">Agents</h5>
                <h2>@_agents.Count</h2>
                <p class="text-muted">@_agents.Count(a => a.IsOnline) online</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">Jobs</h5>
                <h2>@_jobs.Count</h2>
                <p class="text-muted">@_jobs.Count(j => j.Status == JobStatus.Running) running</p>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card">
            <div class="card-body">
                <h5 class="card-title">Builds Today</h5>
                <h2>@_buildsToday</h2>
                <p class="text-muted">Last 24 hours</p>
            </div>
        </div>
    </div>
</div>

<h3 class="mt-4">Recent Agents</h3>
<table class="table table-striped">
    <thead>
        <tr>
            <th>Name</th>
            <th>Machine</th>
            <th>Status</th>
            <th>Last Seen</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var agent in _agents.Take(10))
        {
            <tr>
                <td>@agent.Name</td>
                <td>@agent.MachineName</td>
                <td>
                    @if (agent.IsOnline)
                    {
                        <span class="badge bg-success">Online</span>
                    }
                    else
                    {
                        <span class="badge bg-secondary">Offline</span>
                    }
                </td>
                <td>@agent.LastHeartbeat.ToString("g")</td>
            </tr>
        }
    </tbody>
</table>

@code {
    private List<Agent> _agents = new();
    private List<Job> _jobs = new();
    private int _buildsToday = 0;

    protected override async Task OnInitializedAsync()
    {
        _agents = await DB.Agents.OrderByDescending(a => a.LastHeartbeat).ToListAsync();
        _jobs = await DB.Jobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
        _buildsToday = await DB.Jobs.CountAsync(j => j.CreatedAt >= DateTime.UtcNow.AddDays(-1) && j.Status == JobStatus.Completed);
    }
}
```

---

## ✅ Test It!

```bash
cd C:/build-server/BuildServer
dotnet restore
dotnet run
```

Abre: http://localhost:5000

Deberías ver:
- ✅ Dashboard con stats (0 agents, 0 jobs por ahora)
- ✅ Tabla vacía de agents
- ✅ Swagger en http://localhost:5000/swagger

---

## 🧪 Test API

```bash
# Register an agent
curl -X POST http://localhost:5000/api/agents/register \
  -H "Content-Type: application/json" \
  -d '{
    "agentId": "test-agent-1",
    "name": "Test Agent",
    "machineName": "TEST-PC"
  }'

# Get all agents
curl http://localhost:5000/api/agents
```

Refresh dashboard → Deberías ver el agente!

---

## 📝 Next Steps

Una vez que funcione el MVP:

1. **Crear más páginas Blazor:**
   - `Agents.razor` - Lista y configuración de agentes
   - `Jobs.razor` - Crear y monitorear jobs
   - `Builds.razor` - Historial de builds

2. **Crear más controllers:**
   - `JobsController.cs` - CRUD de jobs
   - `ConfigController.cs` - Configuración de agentes

3. **Agregar SignalR:**
   - Crear `Hubs/BuildHub.cs`
   - Agregar real-time updates

4. **Crear Local Agent:**
   - Windows Service que se conecta al server
   - Descarga config desde el server
   - Ejecuta builds

5. **Simplificar Instalador:**
   - Solo pregunta URL del server
   - Todo lo demás se configura desde web

---

## 📚 Resources

- **README.md** - Documentación completa
- **Blazor Docs** - https://docs.microsoft.com/aspnet/core/blazor/
- **EF Core Docs** - https://docs.microsoft.com/ef/core/
- **SignalR Docs** - https://docs.microsoft.com/aspnet/core/signalr/

---

**Status:** ✅ Proyecto creado, listo para implementar MVP

**Time to MVP:** ~1 hora de código siguiendo esta guía

**Next:** Subir a GitHub y empezar a implementar! 🚀
