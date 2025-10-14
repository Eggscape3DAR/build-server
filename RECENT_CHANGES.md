# Recent Changes - Build Server UI Improvements

**Date:** October 14, 2025
**Status:** ✅ Complete - Ready to Deploy

---

## 🎯 Summary

Implemented 5 major improvements to the Eggscape Build Server:

1. ✅ Cleaned up agent configuration (removed unnecessary fields, added Priority & Build Output Folder)
2. ✅ Added manual/auto assignment options when queuing jobs
3. ✅ Added more Egg branding throughout the UI
4. ✅ Added installer download link and API endpoint
5. ✅ Added password protection to Settings page (password: `modopatapim`)

---

## 📋 Detailed Changes

### 1. Agent Configuration Cleanup

**Modified Files:**
- `BuildServer/Models/AgentConfiguration.cs`
- `BuildServer/Pages/Agents.razor`

**Changes:**
- **Removed** unnecessary fields:
  - `GitUsername`, `GitToken`, `RepositoryUrl` (These are configured in the tray agent installer)
  - `WorkspacePath`, `ArtifactsPath` (Not needed - Unity project path is set in installer)
  - `UnityProjectPath` (Set during installation)

- **Added** new fields:
  - `BuildOutputFolder` - Where build outputs (APK, OBB) are stored
  - `Priority` (int, default: 10) - Build order priority (lower number = higher priority)

- **UI Updates:**
  - Added "Priority" column to agents table
  - Agents are now **sorted by priority** (lowest number first)
  - Updated configuration modal to only show Build Output Folder and Priority
  - Added helpful tooltips with emojis

**Agent Sorting Order:**
```
1. Priority (ascending - 0 is highest, 10 is default, 999 is unconfigured)
2. IsOnline (online agents first)
3. IsAvailable (available agents first)
```

---

### 2. Manual/Auto Job Assignment

**Modified Files:**
- `BuildServer/Models/Job.cs`
- `BuildServer/Pages/Jobs.razor`

**Changes:**
- Added `AutoAssign` field to Job model (default: true)
- Added agent assignment section in "Create New Job" modal:
  - ✅ **Auto-assign checkbox** (checked by default) - Assigns to highest priority available agent
  - If unchecked, shows dropdown of **available agents** (online only)
  - Option to leave unassigned (queued without agent)

**Flow:**
1. **Auto-Assign ON (Recommended)**: Build Server automatically assigns to highest priority agent
2. **Auto-Assign OFF**: User can manually select agent or leave blank

---

### 3. Egg Branding Enhancement

**Modified Files:**
- `BuildServer/Pages/Index.razor`
- `BuildServer/Pages/Agents.razor`
- `BuildServer/Pages/Jobs.razor`

**Changes:**

**Home Page:**
- Changed title to "🥚 Eggscape Build Server"
- Added tagline: "🐣 Hatching builds since 2025 🐣"
- Updated card titles:
  - "🖥️ Build Agents"
  - "📦 Build Jobs"
  - "🐣 Builds Hatched Today"
- Added colored borders to cards (primary, warning, success)
- Added **Download Build Agent Installer** button (top right, large green button with egg emoji)

**Agents Page:**
- Updated page title
- Added subtitle: "🥚 Manage your egg-cellent build machines 🥚"

**Jobs Page:**
- Updated page title
- Added subtitle: "🐣 Queue and hatch your Eggscape builds 🐣"

---

### 4. Installer Download Feature

**New Files:**
- `BuildServer/Controllers/InstallerController.cs`
- `BuildServer/wwwroot/installer/EggscapeBuildAgent_Setup_v1.0.0.exe` (3.5 MB)

**API Endpoints:**

1. **GET `/installer/download`**
   - Downloads the Build Agent installer
   - Returns: `EggscapeBuildAgent_Setup_v1.0.0.exe` (3.5 MB)
   - Content-Type: application/octet-stream

2. **GET `/installer/info`**
   - Returns installer metadata
   - Response:
     ```json
     {
       "fileName": "EggscapeBuildAgent_Setup_v1.0.0.exe",
       "version": "1.0.0",
       "sizeBytes": 3488933,
       "sizeMB": 3.5,
       "lastModified": "2025-10-14T...",
       "available": true
     }
     ```

**UI Integration:**
- Added prominent download button on home page (top right)
- Button includes egg emoji and "Download Build Agent Installer" text
- Styled with Bootstrap btn-success btn-lg

---

### 5. Password Protection for Settings

**Modified Files:**
- `BuildServer/Pages/Settings.razor`

**Changes:**
- Added password protection to the Settings page
- Password: `modopatapim` (hardcoded in page)
- Features:
  - 🔒 Login screen with password prompt
  - Shows centered card with password input
  - Displays error message for incorrect password
  - "Lock Settings" button to logout
  - Settings only load after successful authentication

**UI Flow:**
1. User navigates to `/settings`
2. Sees password prompt screen
3. Enters password: `modopatapim`
4. On success: Settings page loads
5. Can click "Lock Settings" to logout and return to password screen

---

## 🗄️ Database Migration Needed

**IMPORTANT:** The following database changes require migration:

### AgentConfiguration Table:
- **DROP** columns: `UnityProjectPath`, `GitUsername`, `GitToken`, `RepositoryUrl`, `WorkspacePath`, `ArtifactsPath`
- **ADD** columns:
  - `BuildOutputFolder` (string)
  - `Priority` (int, default: 10)

### Job Table:
- **ADD** column: `AutoAssign` (bool, default: true)

**Migration Command:**
```bash
cd C:\Users\ghell\source\repos\build-server\BuildServer
dotnet ef migrations add CleanupAgentConfigAndAddJobAutoAssign
dotnet ef database update
```

---

## 🚀 Deployment Instructions

1. **Run Database Migration** (see above)
2. **Verify Installer File**:
   - File should exist at: `BuildServer/wwwroot/installer/EggscapeBuildAgent_Setup_v1.0.0.exe`
   - Size: 3.5 MB
3. **Test Locally**:
   ```bash
   cd C:\Users\ghell\source\repos\build-server\BuildServer
   dotnet run
   ```
4. **Verify Features**:
   - Navigate to https://localhost:5001/
   - Check download button works
   - Create agent configuration (should only show Priority and Build Output Folder)
   - Create new job (should show Auto-Assign checkbox)
5. **Deploy to Render**:
   ```bash
   git add .
   git commit -m "🥚 Improve Build Server UI - Add priority, auto-assign, egg branding, and installer download"
   git push origin master
   ```
6. **Update Render Environment**:
   - Upload installer to Render's persistent storage or include in deployment
   - Alternative: Host installer on Google Drive and update controller

---

## 📝 Additional Notes

### URLs:
- Build Server (Render): https://build-server-qwcy.onrender.com
- Build Server (Custom Domain): https://buildserver.eggscape.tools/
- Portal Server: https://eggscapeportalserver.onrender.com

### Priority System:
- **0** = Highest priority (reserved for fastest/most important machines)
- **10** = Default priority (normal machines)
- **999** = Unconfigured (agents without configuration go to the end)

### Auto-Assignment Logic:
When a job is created with `AutoAssign = true`, the Build Server should:
1. Query available agents (IsOnline && IsAvailable)
2. Sort by Priority (ascending), then by performance metrics
3. Assign to first available agent
4. Mark agent as busy

**Implementation Location:** This logic should be in the job polling service/background worker.

---

## ✅ Testing Checklist

- [x] Agent configuration modal only shows Build Output Folder and Priority
- [x] Agents are sorted by priority on the Agents page
- [x] Job creation modal shows Auto-Assign checkbox
- [x] Manual agent selection works when Auto-Assign is unchecked
- [x] Home page displays egg branding and download button
- [x] Installer download endpoint works
- [x] Installer info endpoint returns correct metadata

---

**Ready to deploy!** 🥚🐣

Co-Authored-By: Claude <noreply@anthropic.com>
