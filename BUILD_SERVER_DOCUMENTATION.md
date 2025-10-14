# Build Server - Complete Technical Documentation

**Version:** 1.0.0
**Technology:** ASP.NET Core 6 + Blazor Server + SignalR
**Database:** SQLite with Entity Framework Core
**Deployment:** Docker + Linux/Windows Server

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [Data Models](#data-models)
4. [API Endpoints](#api-endpoints)
5. [Web UI (Blazor Pages)](#web-ui-blazor-pages)
6. [Real-time Communication (SignalR)](#real-time-communication-signalr)
7. [Services](#services)
8. [Database Schema](#database-schema)
9. [Configuration](#configuration)
10. [Deployment](#deployment)
11. [Integration with Build Agent](#integration-with-build-agent)

---

## System Overview

The **Build Server** is the central orchestration hub for the Eggscape automated build system. It provides:

- **Web UI** for managing builds, agents, and jobs
- **REST API** for programmatic access
- **SignalR Hub** for real-time updates
- **SQLite Database** for persistence
- **Agent Management** with heartbeat monitoring
- **Job Queue** with automatic agent assignment

### Key Responsibilities

1. **Job Management**
   - Create, queue, and track build jobs
   - Assign jobs to available agents
   - Monitor progress and completion
   - Store job history

2. **Agent Management**
   - Register and track build agents
   - Monitor agent health via heartbeat
   - Track agent performance metrics
   - Manage agent availability

3. **Real-time Updates**
   - Broadcast job progress to web clients
   - Notify on agent status changes
   - Live dashboard updates

4. **Configuration Storage**
   - Global settings (Git credentials, Google Drive)
   - Per-agent configuration
   - Build profiles and defaults

---

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      WEB BROWSER                             │
│                 http://buildserver.eggscape.tools            │
│                                                              │
│  Blazor Pages:                                              │
│  • Index.razor (Dashboard)                                  │
│  • Jobs.razor (Job creation & monitoring)                   │
│  • Agents.razor (Agent management)                          │
│  • Settings.razor (Global configuration)                    │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   │ HTTP/SignalR
                   ▼
┌─────────────────────────────────────────────────────────────┐
│               BUILD SERVER (ASP.NET Core)                    │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Controllers (REST API)                              │  │
│  │  • JobsController - Job CRUD operations              │  │
│  │  • AgentsController - Agent registration/heartbeat   │  │
│  │  • ConfigController - Configuration management       │  │
│  │  • GitController - Git operations                    │  │
│  │  • VersionController - API version info              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Hubs (SignalR)                                      │  │
│  │  • BuildHub - Real-time job/agent updates           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Services                                            │  │
│  │  • AgentHealthCheckService - Background monitoring   │  │
│  │  • GitService - Git operations helper               │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Data Layer (EF Core)                                │  │
│  │  • BuildServerContext - DbContext                    │  │
│  │  • SQLite Database                                   │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   │ HTTP Polling
                   ▼
┌─────────────────────────────────────────────────────────────┐
│                    BUILD AGENT                               │
│              (Desktop WPF Application)                       │
│                                                              │
│  • Polls /api/agents/poll for new jobs                     │
│  • Sends heartbeat to /api/agents/heartbeat                │
│  • Reports progress to /api/jobs/{id}/progress             │
│  • Reports completion to /api/jobs/{id}/complete           │
└─────────────────────────────────────────────────────────────┘
```

### Technology Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor Server (C# Razor components) |
| **Backend** | ASP.NET Core 6 Web API |
| **Database** | SQLite 3 with EF Core 6 |
| **Real-time** | SignalR WebSockets |
| **API Docs** | Swagger/OpenAPI |
| **Deployment** | Docker (Linux) or IIS (Windows) |

---

## Data Models

### Agent Model

Represents a build agent machine.

```csharp
public class Agent
{
    public int Id { get; set; }
    public string AgentId { get; set; }           // Unique identifier (GUID)
    public string Name { get; set; }              // Human-readable name
    public string MachineName { get; set; }       // Computer name
    public string IpAddress { get; set; }         // IP address
    public bool IsOnline { get; set; }            // Currently online?
    public bool IsAvailable { get; set; }         // Available for jobs?
    public DateTime LastHeartbeat { get; set; }   // Last heartbeat timestamp
    public string? CurrentJobId { get; set; }     // Currently assigned job
    public DateTime CreatedAt { get; set; }       // Registration time

    // Performance Metrics
    public int? LastBuildDurationSeconds { get; set; }
    public int? AverageBuildDurationSeconds { get; set; }
    public int TotalBuildsCompleted { get; set; }
}
```

**Purpose:** Track build agents and their availability

**Key Fields:**
- `IsOnline` - Set to true when heartbeat received within timeout window
- `IsAvailable` - Agent can accept new jobs (not busy)
- `CurrentJobId` - Reference to job currently being processed
- Performance metrics calculated automatically on job completion

---

### Job Model

Represents a build job.

```csharp
public class Job
{
    public int Id { get; set; }
    public string JobId { get; set; }             // Unique identifier (GUID)
    public string Name { get; set; }              // Job name
    public string ProfileName { get; set; }       // Build profile (e.g., "MetaQuest")
    public string Platform { get; set; }          // Target platform (e.g., "Android")
    public string Channel { get; set; }           // Distribution channel

    // Git Information
    public string GitBranch { get; set; }
    public string GitCommitHash { get; set; }
    public string GitCommitMessage { get; set; }
    public string GitCommitAuthor { get; set; }
    public DateTime? GitCommitDate { get; set; }

    // Build Options
    public bool UploadToGoogleDrive { get; set; } = true;
    public bool UploadToChannel { get; set; } = false;
    public string BuildType { get; set; } = "LevelBuilder";
    public string AppVersion { get; set; } = "1.0.0";
    public int BundleCode { get; set; } = 1;

    // Status
    public JobStatus Status { get; set; }         // Queued, Running, Completed, etc.
    public float Progress { get; set; }           // 0.0 to 1.0
    public string? AssignedAgentId { get; set; }  // Assigned agent
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum JobStatus
{
    Queued,      // Waiting for agent
    Assigned,    // Assigned to agent, not started
    Running,     // Currently building
    Completed,   // Successfully completed
    Failed,      // Build failed
    Cancelled    // Cancelled by user
}
```

**Purpose:** Store build job details and track execution

**Key Fields:**
- `ProfileName` - Maps to Unity Build Wizard profile
- `Platform` - Target platform (Android, iOS, etc.)
- `GitBranch` - Branch to build from
- `UploadToGoogleDrive` - Should upload artifacts to Google Drive
- `Status` - Current job state
- `Progress` - Build progress (0-100%)

---

### GlobalSettings Model

System-wide configuration.

```csharp
public class GlobalSettings
{
    public int Id { get; set; }
    public string? GitUsername { get; set; }
    public string? GitToken { get; set; }
    public string? PortalServerUrl { get; set; }
    public string? GoogleDriveCredentialsJson { get; set; }
    public string? GoogleDriveFolderId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**Purpose:** Store global configuration (Git credentials, Google Drive settings)

---

## API Endpoints

### Jobs API

#### Create Job
```http
POST /api/jobs
Content-Type: application/json

{
  "name": "Android Production Build",
  "profileName": "MetaQuest",
  "platform": "Android",
  "channel": "production",
  "gitBranch": "main",
  "gitCommitHash": "abc123",
  "uploadToGoogleDrive": true,
  "appVersion": "1.0.0",
  "bundleCode": 100
}

Response: 200 OK
{
  "jobId": "guid-here",
  "message": "Job created successfully"
}
```

#### Get All Jobs
```http
GET /api/jobs

Response: 200 OK
[
  {
    "id": 1,
    "jobId": "guid",
    "name": "Android Production Build",
    "status": "Completed",
    "progress": 1.0,
    ...
  }
]
```

#### Get Next Job (for Agents)
```http
GET /api/jobs/queue?agentId=agent-guid

Response: 200 OK (if job available)
{
  "jobId": "guid",
  "name": "...",
  "profileName": "MetaQuest",
  ...
}

Response: 404 Not Found (if queue empty)
{
  "message": "No jobs in queue"
}
```

**Behavior:**
- Finds first job with status `Queued`
- Changes status to `Assigned`
- Sets `AssignedAgentId` to requesting agent
- Sets `StartedAt` timestamp

#### Update Progress
```http
POST /api/jobs/{jobId}/progress
Content-Type: application/json

{
  "progress": 0.45
}

Response: 200 OK
{
  "message": "Progress updated"
}
```

**Behavior:**
- Updates job status to `Running`
- Sets progress value (0.0 to 1.0)

#### Complete Job
```http
POST /api/jobs/{jobId}/complete
Content-Type: application/json

{
  "buildPath": "/path/to/build"
}

Response: 200 OK
{
  "message": "Job completed successfully"
}
```

**Behavior:**
- Sets status to `Completed`
- Sets progress to 1.0
- Sets `CompletedAt` timestamp
- Marks agent as available
- Updates agent performance metrics

#### Fail Job
```http
POST /api/jobs/{jobId}/fail
Content-Type: application/json

{
  "errorMessage": "Build failed: Unity crashed"
}

Response: 200 OK
{
  "message": "Job marked as failed"
}
```

---

### Agents API

#### Register Agent
```http
POST /api/agents/register
Content-Type: application/json

{
  "agentId": "agent-guid",
  "name": "BuildMachine01",
  "machineName": "DESKTOP-ABC123",
  "ipAddress": "192.168.1.100"
}

Response: 200 OK
{
  "message": "Agent registered successfully"
}
```

**Behavior:**
- Creates new agent or updates existing one
- Sets `IsOnline` to true
- Sets `LastHeartbeat` to current time

#### Send Heartbeat
```http
POST /api/agents/heartbeat
Content-Type: application/json

{
  "agentId": "agent-guid",
  "isAvailable": true,
  "currentJobId": "job-guid-or-null"
}

Response: 200 OK
{
  "message": "Heartbeat received"
}
```

**Behavior:**
- Updates `LastHeartbeat` timestamp
- Sets `IsOnline` to true
- Updates `IsAvailable` and `CurrentJobId`

#### Get All Agents
```http
GET /api/agents

Response: 200 OK
[
  {
    "id": 1,
    "agentId": "guid",
    "name": "BuildMachine01",
    "isOnline": true,
    "isAvailable": true,
    ...
  }
]
```

---

### Configuration API

#### Get Agent Config
```http
GET /api/config/{agentId}

Response: 200 OK
{
  "agentId": "guid",
  "unityProjectPath": "C:\\Projects\\eggscape",
  "repositoryUrl": "https://github.com/Eggscape3DAR/eggscape.git",
  "workspacePath": "C:\\Workspace",
  ...
}
```

#### Update Agent Config
```http
POST /api/config/{agentId}
Content-Type: application/json

{
  "unityProjectPath": "C:\\Projects\\eggscape",
  "repositoryUrl": "https://github.com/Eggscape3DAR/eggscape.git",
  ...
}

Response: 200 OK
{
  "message": "Configuration updated"
}
```

---

## Web UI (Blazor Pages)

### Dashboard (Index.razor)

**URL:** `/`

**Purpose:** Overview of system status

**Features:**
- Agent count (online/total)
- Job count (running/total)
- Builds completed today
- Recent agents table with status
- Link to Swagger API docs

**Code:**
```csharp
@page "/"
@inject BuildServerContext DB

protected override async Task OnInitializedAsync()
{
    _agents = await DB.Agents.OrderByDescending(a => a.LastHeartbeat).ToListAsync();
    _jobs = await DB.Jobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
    _buildsToday = await DB.Jobs.CountAsync(j =>
        j.CreatedAt >= DateTime.UtcNow.AddDays(-1) &&
        j.Status == JobStatus.Completed);
}
```

---

### Jobs Page (Jobs.razor)

**URL:** `/jobs`

**Purpose:** Create and monitor build jobs

**Features:**
- Create new job button with form
- List of all jobs with status
- Real-time progress updates (SignalR)
- Filter by status
- Cancel/Delete jobs

---

### Agents Page (Agents.razor)

**URL:** `/agents`

**Purpose:** Manage build agents

**Features:**
- List of all registered agents
- Online/Offline/Busy status indicators
- Last heartbeat timestamp
- Performance metrics (avg build time, total builds)
- Configure agent button
- Enable/Disable agent

---

### Settings Page (Settings.razor)

**URL:** `/settings`

**Purpose:** Global system configuration

**Features:**
- Git credentials (username/token)
- Google Drive settings (credentials JSON, folder ID)
- Portal server URL
- Save button to persist changes

---

## Real-time Communication (SignalR)

### BuildHub

**URL:** `/hubs/build`

**Purpose:** Real-time updates to web clients

### Client → Server Methods

```javascript
// Join a specific job group to receive updates
connection.invoke("JoinJobGroup", "job-guid");

// Leave a job group
connection.invoke("LeaveJobGroup", "job-guid");
```

### Server → Client Events

```javascript
// Agent status changed
connection.on("AgentUpdated", (data) => {
  // data: { agentId, isOnline, isAvailable, timestamp }
});

// Job progress updated
connection.on("JobProgressUpdated", (data) => {
  // data: { jobId, progress, currentStep, timestamp }
});

// Job completed
connection.on("JobCompleted", (data) => {
  // data: { jobId, success, buildPath, timestamp }
});

// New job created
connection.on("JobCreated", (data) => {
  // data: { jobId, name, timestamp }
});
```

### JavaScript Client Example

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/build")
    .withAutomaticReconnect()
    .build();

connection.on("JobProgressUpdated", (data) => {
    console.log(`Job ${data.jobId}: ${data.progress * 100}%`);
    updateProgressBar(data.jobId, data.progress);
});

await connection.start();
console.log("Connected to SignalR hub");
```

---

## Services

### AgentHealthCheckService

**Type:** `IHostedService` (Background Service)

**Purpose:** Monitor agent health and mark offline agents

**Behavior:**
- Runs every 10 seconds
- Checks all agents' `LastHeartbeat` timestamp
- If heartbeat older than 30 seconds → mark agent as offline
- If heartbeat older than 60 seconds → mark job as failed (if agent has job)

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        await CheckAgentHealthAsync();
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
    }
}

private async Task CheckAgentHealthAsync()
{
    var timeout = DateTime.UtcNow.AddSeconds(-30);
    var offlineAgents = await _db.Agents
        .Where(a => a.IsOnline && a.LastHeartbeat < timeout)
        .ToListAsync();

    foreach (var agent in offlineAgents)
    {
        agent.IsOnline = false;
        agent.IsAvailable = false;

        // If agent has job, mark it as failed
        if (!string.IsNullOrEmpty(agent.CurrentJobId))
        {
            var job = await _db.Jobs.FirstOrDefaultAsync(j => j.JobId == agent.CurrentJobId);
            if (job != null && job.Status == JobStatus.Running)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Agent went offline during build";
            }
        }
    }

    await _db.SaveChangesAsync();
}
```

---

### GitService

**Purpose:** Helper methods for Git operations

**Methods:**
```csharp
Task<string> GetCurrentCommitAsync(string repoPath);
Task<string> GetCurrentBranchAsync(string repoPath);
Task<string> GetCommitMessageAsync(string repoPath, string commitHash);
```

---

## Database Schema

### SQLite Database: `buildserver.db`

**Location:**
- Development: `./buildserver.db`
- Docker: `/app/data/buildserver.db`

### Tables

#### Agents
```sql
CREATE TABLE Agents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AgentId TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    MachineName TEXT,
    IpAddress TEXT,
    IsOnline INTEGER NOT NULL DEFAULT 0,
    IsAvailable INTEGER NOT NULL DEFAULT 1,
    LastHeartbeat DATETIME NOT NULL,
    CurrentJobId TEXT,
    CreatedAt DATETIME NOT NULL,
    LastBuildDurationSeconds INTEGER,
    AverageBuildDurationSeconds INTEGER,
    TotalBuildsCompleted INTEGER NOT NULL DEFAULT 0
);
```

#### Jobs
```sql
CREATE TABLE Jobs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    JobId TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    ProfileName TEXT NOT NULL,
    Platform TEXT NOT NULL,
    Channel TEXT NOT NULL,
    GitBranch TEXT,
    GitCommitHash TEXT,
    GitCommitMessage TEXT,
    GitCommitAuthor TEXT,
    GitCommitDate DATETIME,
    UploadToGoogleDrive INTEGER NOT NULL DEFAULT 1,
    UploadToChannel INTEGER NOT NULL DEFAULT 0,
    BuildType TEXT NOT NULL,
    AppVersion TEXT NOT NULL,
    BundleCode INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    Progress REAL NOT NULL DEFAULT 0,
    AssignedAgentId TEXT,
    CreatedAt DATETIME NOT NULL,
    StartedAt DATETIME,
    CompletedAt DATETIME,
    ErrorMessage TEXT
);
```

#### GlobalSettings
```sql
CREATE TABLE GlobalSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GitUsername TEXT,
    GitToken TEXT,
    PortalServerUrl TEXT,
    GoogleDriveCredentialsJson TEXT,
    GoogleDriveFolderId TEXT,
    UpdatedAt DATETIME NOT NULL
);
```

#### AgentConfigurations
```sql
CREATE TABLE AgentConfigurations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AgentId TEXT NOT NULL UNIQUE,
    UpdatedAt DATETIME NOT NULL
);
```

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/app/data/buildserver.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variables (Docker)

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
ConnectionStrings__DefaultConnection=Data Source=/app/data/buildserver.db
```

---

## Deployment

### Docker Deployment

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BuildServer/BuildServer.csproj", "BuildServer/"]
RUN dotnet restore "BuildServer/BuildServer.csproj"
COPY . .
WORKDIR "/src/BuildServer"
RUN dotnet build "BuildServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BuildServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create data directory for SQLite
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "BuildServer.dll"]
```

**Docker Compose:**
```yaml
version: '3.8'
services:
  buildserver:
    build: .
    ports:
      - "5000:80"
    volumes:
      - ./data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/buildserver.db
    restart: unless-stopped
```

**Commands:**
```bash
# Build image
docker build -t eggscape-buildserver .

# Run container
docker run -d -p 5000:80 -v $(pwd)/data:/app/data eggscape-buildserver

# Or use docker-compose
docker-compose up -d
```

---

## Integration with Build Agent

### Agent Workflow

```
1. Agent starts → Register with server
   POST /api/agents/register

2. Start heartbeat loop (every 5 seconds)
   POST /api/agents/heartbeat

3. Poll for jobs (every 2 seconds)
   GET /api/jobs/queue?agentId=xxx

4. If job assigned:
   a. Update Git repository
   b. Launch Unity
   c. Start build
   d. Report progress (every 2 seconds)
      POST /api/jobs/{jobId}/progress
   e. Upload to Google Drive
   f. Report completion
      POST /api/jobs/{jobId}/complete

5. Loop back to step 3
```

### Communication Protocol

**Agent → Server:**
- Heartbeat every 5 seconds
- Job polling every 2 seconds
- Progress updates every 2 seconds during build

**Server → Agent:**
- Job assignment (response to poll)
- Configuration updates (response to config request)

---

## API Documentation (Swagger)

Access interactive API documentation at:

**Development:** `http://localhost:5000/swagger`
**Production:** `https://buildserver.eggscape.tools/swagger`

Swagger UI provides:
- Complete endpoint listing
- Request/response schemas
- Try it out functionality
- Model definitions

---

## Security Considerations

### Current State (MVP)

- ✅ SQLite database with file-based storage
- ✅ Agent identification via AgentId
- ❌ No authentication on API endpoints
- ❌ No authorization/role-based access
- ❌ Git credentials stored in plain text
- ❌ No HTTPS enforcement

### Future Enhancements

1. **Add JWT Authentication**
   - User accounts with login
   - Role-based access (Admin, Builder, Viewer)
   - Protected API endpoints

2. **Encrypt Sensitive Data**
   - Git credentials encrypted at rest
   - Google Drive credentials encrypted

3. **HTTPS Only**
   - Force HTTPS in production
   - Add SSL certificates

4. **API Keys for Agents**
   - Each agent has unique API key
   - Rotate keys periodically

---

## Monitoring & Logging

### Built-in Logging

ASP.NET Core logging configured in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Log Locations

**Development:** Console output
**Docker:** Container logs (`docker logs <container>`)
**Production:** Configured log provider (e.g., file, Serilog)

### Key Events Logged

- Agent registration
- Agent heartbeat
- Job creation
- Job assignment
- Job progress updates
- Job completion/failure
- Agent going offline

---

## Troubleshooting

### Agent Not Appearing in Dashboard

**Check:**
1. Agent is running and sending heartbeats
2. Server is accessible from agent machine
3. No firewall blocking port 5000
4. Check server logs for registration errors

### Jobs Stuck in Queued Status

**Check:**
1. At least one agent is online and available
2. Agent is polling for jobs (`/api/jobs/queue`)
3. Agent heartbeat is being received
4. Check database: `SELECT * FROM Agents WHERE IsAvailable = 1`

### Database Migration Errors

**Solution:**
Delete database and restart:
```bash
rm buildserver.db
dotnet run
```

---

*Last Updated: 2025-01-14*
*Version: 1.0.0*
