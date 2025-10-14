using Microsoft.EntityFrameworkCore;
using BuildServer.Data;
using BuildServer.Hubs;
using BuildServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
builder.Services.AddSignalR(); // Add SignalR

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=/app/data/buildserver.db";
builder.Services.AddDbContext<BuildServerContext>(options =>
    options.UseSqlite(connectionString));

// Application services
builder.Services.AddScoped<GitService>();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request?.Scheme + "://" +
                          sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request?.Host ?? "http://localhost:5000")
});
builder.Services.AddHttpContextAccessor();

// Background services
builder.Services.AddHostedService<AgentHealthCheckService>();

// Swagger (API docs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Build Server API", Version = "v1" });
});

var app = builder.Build();

// Apply database migrations with conflict resolution
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BuildServerContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("already exists"))
    {
        logger.LogWarning("Migration conflict detected - database was created without migrations tracking");
        logger.LogWarning("Deleting old database and recreating with migrations...");

        // Close and dispose the current database connection
        logger.LogInformation("Closing existing database connection...");
        db.Database.CloseConnection();
        scope.Dispose();

        // Delete the old database files
        var dbPath = "/app/data/buildserver.db";
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
            logger.LogInformation("Deleted old database file");
        }

        // Delete WAL and SHM files if they exist
        if (File.Exists(dbPath + "-wal"))
        {
            File.Delete(dbPath + "-wal");
            logger.LogInformation("Deleted WAL file");
        }
        if (File.Exists(dbPath + "-shm"))
        {
            File.Delete(dbPath + "-shm");
            logger.LogInformation("Deleted SHM file");
        }

        // Small delay to ensure files are fully released
        System.Threading.Thread.Sleep(100);

        // Create a completely new scope and context for clean migration
        logger.LogInformation("Creating fresh database context...");
        using var newScope = app.Services.CreateScope();
        var newDb = newScope.ServiceProvider.GetRequiredService<BuildServerContext>();

        // Retry migration with fresh context
        logger.LogInformation("Applying migrations to new database...");
        newDb.Database.Migrate();
        logger.LogInformation("Database recreated successfully with migrations");
    }
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

// Always enable Swagger in production for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Build Server API v1");
});

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<BuildHub>("/hubs/build"); // Map SignalR hub
app.MapFallbackToPage("/_Host");
app.MapControllers();

app.Run();
