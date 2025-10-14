# 🚀 Eggscape Build Server

**Centralized build orchestration server with web UI**

Version 1.0.0

---

## 📋 What Is This?

A **web-based build orchestration system** for Unity projects. Agents register themselves, and you configure everything from the web UI.

### Architecture

```
┌────────────────────────────────────────────────┐
│         WEB BROWSER                             │
│      http://buildserver:5000                    │
│                                                 │
│  Pages:                                        │
│  - Dashboard (view all agents + jobs)          │
│  - Agents (configure agents)                   │
│  - Jobs (create/monitor jobs)                  │
│  - Settings (Git, Unity paths, etc.)           │
└─────────────────┬──────────────────────────────┘
                  │ HTTP/SignalR
                  ↓
┌────────────────────────────────────────────────┐
│      BUILD SERVER (ASP.NET + Blazor)           │
│                                                │
│  - REST API                                    │
│  - SignalR Hub (real-time)                     │
│  - SQLite Database                              │
│  - Web UI (Blazor Server)                      │
└─────────────────┬──────────────────────────────┘
                  │ HTTP (polling)
                  ↓
┌────────────────────────────────────────────────┐
│      LOCAL AGENT (Windows Service)             │
│                                                │
│  - Registers with server on startup            │
│  - Polls for jobs every 2 seconds             │
│  - Downloads config from server                │
│  - Launches Unity                              │
│  - Reports progress                            │
└─────────────────┬──────────────────────────────┘
                  │ Process
                  ↓
┌────────────────────────────────────────────────┐
│      UNITY EDITOR + BUILD AGENT                │
│                                                │
│  - Executes builds via Build Wizard            │
└────────────────────────────────────────────────┘
```

---

## 🎯 Key Features

### For Users (Web UI)
✅ **Dashboard** - See all agents and jobs at a glance
✅ **Agent Management** - Register, configure, enable/disable agents
✅ **Job Creation** - Create builds with web forms
✅ **Real-time Progress** - Watch builds in real-time (SignalR)
✅ **Build History** - View past builds and download artifacts
✅ **Configuration** - All settings via web (no more manual config files!)

### For Agents
✅ **Auto-registration** - Agent registers itself on startup
✅ **Auto-configuration** - Downloads config from server
✅ **Heartbeat** - Reports status every 5 seconds
✅ **Job polling** - Checks for new jobs every 2 seconds
✅ **Progress reporting** - Sends updates in real-time

---

## 📦 Project Structure

```
build-server/
├── BuildServer/                     # Main ASP.NET + Blazor project
│   │
│   ├── Models/                      # Data models
│   │   ├── Agent.cs                 # Agent model
│   │   ├── Job.cs                   # Job model
│   │   ├── Build.cs                 # Build result model
│   │   └── AgentConfiguration.cs    # Agent config model
│   │
│   ├── Data/                        # Database
│   │   ├── BuildServerContext.cs    # EF Core context
│   │   └── Migrations/              # EF migrations
│   │
│   ├── Controllers/                 # REST API
│   │   ├── AgentsController.cs      # Agent endpoints
│   │   ├── JobsController.cs        # Job endpoints
│   │   └── ConfigController.cs      # Configuration endpoints
│   │
│   ├── Hubs/                        # SignalR
│   │   └── BuildHub.cs              # Real-time updates
│   │
│   ├── Services/                    # Business logic
│   │   ├── AgentService.cs          # Agent management
│   │   ├── JobQueueService.cs       # Job scheduling
│   │   └── ConfigService.cs         # Configuration management
│   │
│   ├── Pages/                       # Blazor pages (WEB UI)
│   │   ├── Index.razor              # Dashboard
│   │   ├── Agents.razor             # Agent list + config
│   │   ├── Jobs.razor               # Job creation + monitoring
│   │   ├── Builds.razor             # Build history
│   │   └── Settings.razor           # Global settings
│   │
│   ├── Shared/                      # Shared components
│   │   ├── MainLayout.razor         # Layout
│   │   └── NavMenu.razor            # Navigation
│   │
│   ├── wwwroot/                     # Static files
│   │   ├── css/                     # Styles
│   │   └── js/                      # JavaScript
│   │
│   ├── appsettings.json             # Configuration
│   ├── Program.cs                   # Entry point
│   └── BuildServer.csproj           # Project file
│
├── .gitignore                       # Git ignore
├── README.md                        # This file
└── LICENSE                          # License
```

---

## 🚀 Quick Start

### Prerequisites
- .NET 6 SDK
- Windows/Linux/Mac (server can run anywhere)

### 1. Clone & Build

```bash
git clone https://github.com/Eggscape3DAR/build-server.git
cd build-server/BuildServer
dotnet restore
dotnet build
```

### 2. Run Server

```bash
dotnet run
```

Server starts at: **http://localhost:5000**

### 3. Open Web UI

Open browser: **http://localhost:5000**

You'll see:
- Dashboard (no agents yet)
- Agents page (empty)
- Jobs page (no jobs yet)

### 4. Install Agent

On build machines:
1. Download installer from: `http://localhost:5000/download/installer`
2. Run installer
3. Enter server URL: `http://localhost:5000`
4. Agent auto-registers and appears in web UI!

### 5. Configure Agent (from Web UI!)

1. Go to **Agents** page
2. Click on your agent
3. Configure:
   - Unity project path
   - Git credentials
   - Repository URL
   - Workspace path
   - Artifacts path
4. Click **Save**

Agent downloads config automatically!

### 6. Create Job (from Web UI!)

1. Go to **Jobs** page
2. Click **New Job**
3. Fill form:
   - Profile (Android Dev APK, etc.)
   - Platform
   - Channel
   - Branch
4. Click **Create**

Job assigned to available agent automatically!

### 7. Watch Progress (Real-time!)

- Dashboard shows live progress
- SignalR updates every second
- See build status, progress bar, current step

---

## 🛠️ API Endpoints

### Agent Endpoints

```
POST /api/agents/register
POST /api/agents/heartbeat
GET  /api/agents
GET  /api/agents/{id}
PUT  /api/agents/{id}
DELETE /api/agents/{id}
```

### Job Endpoints

```
POST /api/jobs
GET  /api/jobs
GET  /api/jobs/{id}
GET  /api/jobs/queue          # Get next job for agent
POST /api/jobs/{id}/progress  # Update progress
POST /api/jobs/{id}/complete  # Mark complete
POST /api/jobs/{id}/fail      # Mark failed
```

### Config Endpoints

```
GET  /api/config/{agentId}     # Get agent config
POST /api/config/{agentId}     # Update agent config
```

### Build Endpoints

```
GET  /api/builds
GET  /api/builds/{id}
GET  /api/builds/{id}/download # Download artifacts
```

---

## 📊 Database Schema

### Agents Table
```sql
CREATE TABLE Agents (
    Id INTEGER PRIMARY KEY,
    AgentId TEXT UNIQUE NOT NULL,
    Name TEXT NOT NULL,
    MachineName TEXT,
    IpAddress TEXT,
    IsOnline BOOLEAN DEFAULT 0,
    IsAvailable BOOLEAN DEFAULT 1,
    LastHeartbeat DATETIME,
    CurrentJobId TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Jobs Table
```sql
CREATE TABLE Jobs (
    Id INTEGER PRIMARY KEY,
    JobId TEXT UNIQUE NOT NULL,
    Name TEXT NOT NULL,
    ProfileName TEXT NOT NULL,
    Platform TEXT NOT NULL,
    Channel TEXT NOT NULL,
    Status TEXT NOT NULL,  -- Queued, Running, Completed, Failed
    Progress REAL DEFAULT 0,
    AssignedAgentId TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    StartedAt DATETIME,
    CompletedAt DATETIME,
    ErrorMessage TEXT
);
```

### AgentConfigurations Table
```sql
CREATE TABLE AgentConfigurations (
    Id INTEGER PRIMARY KEY,
    AgentId TEXT UNIQUE NOT NULL,
    UnityProjectPath TEXT,
    GitUsername TEXT,
    GitToken TEXT,  -- Encrypted!
    RepositoryUrl TEXT,
    WorkspacePath TEXT,
    ArtifactsPath TEXT,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Builds Table
```sql
CREATE TABLE Builds (
    Id INTEGER PRIMARY KEY,
    JobId INTEGER NOT NULL,
    BuildPath TEXT NOT NULL,
    SizeBytes INTEGER,
    Duration INTEGER,  -- seconds
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (JobId) REFERENCES Jobs(Id)
);
```

---

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=buildserver.db"
  },
  "BuildServer": {
    "MaxJobQueueSize": 100,
    "AgentTimeoutSeconds": 30,
    "JobTimeoutMinutes": 120,
    "ArtifactsRetentionDays": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🌐 Web Pages (Blazor)

### Dashboard (`Index.razor`)
Shows:
- Total agents (online/offline)
- Total jobs (queued/running/completed)
- Recent builds
- Live activity feed (SignalR)

### Agents (`Agents.razor`)
- List of all agents
- Status indicators (online/offline/busy)
- Configure button → opens config form
- Enable/Disable toggle
- Delete button

### Jobs (`Jobs.razor`)
- Create new job button → opens form
- Job queue (pending jobs)
- Running jobs with live progress
- Recent jobs

### Builds (`Builds.razor`)
- Build history with filters
- Download artifacts button
- View logs button
- Build details

### Settings (`Settings.razor`)
- Global settings:
  - Default Unity version
  - Default workspace path
  - Git credentials (global)
  - Build retention policy
  - Notification settings

---

## 🔐 Security

### Agent Authentication
- Each agent gets unique `AgentId` on first registration
- Stored in agent's local config
- Required for all API calls

### Git Credentials
- Stored encrypted in database
- Never sent to client (web UI)
- Only sent to agent when needed

### Future: JWT Authentication
- Add user accounts
- Login system
- Role-based access (Admin/Builder/Viewer)

---

## 📈 Real-time Updates (SignalR)

### Events

**From Server to Clients:**
```javascript
// Agent status changed
"AgentUpdated" -> { agentId, isOnline, isAvailable }

// Job progress
"JobProgressUpdated" -> { jobId, progress, currentStep }

// Job completed
"JobCompleted" -> { jobId, success, buildPath }

// New job created
"JobCreated" -> { jobId, name }
```

**From Clients to Server:**
```javascript
// Join specific job updates
"JoinJobGroup" -> { jobId }

// Leave job updates
"LeaveJobGroup" -> { jobId }
```

### Client Usage (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/build")
    .build();

connection.on("JobProgressUpdated", (data) => {
    console.log(`Job ${data.jobId}: ${data.progress * 100}%`);
    updateProgressBar(data.jobId, data.progress);
});

await connection.start();
```

---

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production (Linux)

```bash
# 1. Publish
dotnet publish -c Release -o /var/www/buildserver

# 2. Create systemd service
sudo nano /etc/systemd/system/buildserver.service
```

```ini
[Unit]
Description=Eggscape Build Server
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/buildserver
ExecStart=/usr/bin/dotnet /var/www/buildserver/BuildServer.dll
Restart=always
RestartSec=10
SyslogIdentifier=buildserver
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# 3. Enable and start
sudo systemctl enable buildserver
sudo systemctl start buildserver
```

### Production (Windows)

```bash
# 1. Publish
dotnet publish -c Release -o C:\BuildServer

# 2. Install as Windows Service
sc create BuildServer binPath="C:\BuildServer\BuildServer.exe"
sc start BuildServer
```

---

## 📝 TODO / Roadmap

### MVP (Current)
- [x] Project structure
- [ ] Models and database
- [ ] REST API controllers
- [ ] Blazor web pages
- [ ] SignalR hub
- [ ] Agent registration
- [ ] Job queueing
- [ ] Real-time updates

### V1.1
- [ ] User authentication (JWT)
- [ ] File upload (for keystores, etc.)
- [ ] Artifact storage and download
- [ ] Email notifications
- [ ] Slack/Discord webhooks

### V1.2
- [ ] Build statistics and analytics
- [ ] Agent scheduling (cron jobs)
- [ ] Build templates
- [ ] Multi-branch builds
- [ ] Pull request builds

### V2.0
- [ ] Docker support
- [ ] Kubernetes deployment
- [ ] Cloud storage (S3/Azure/GCS)
- [ ] Advanced metrics
- [ ] Custom plugins

---

## 🤝 Contributing

1. Fork the repo
2. Create feature branch
3. Commit changes
4. Push and create PR

---

## 📞 Support

- **Documentation:** This README
- **Issues:** GitHub Issues
- **Discussions:** GitHub Discussions

---

## 📄 License

MIT License - See LICENSE file

---

**Created:** October 2025
**Version:** 1.0.0
**Project:** Eggscape Build Server
**Team:** Eggscape + Claude

---

**Next:** Complete implementation following `IMPLEMENTATION_GUIDE.md`
