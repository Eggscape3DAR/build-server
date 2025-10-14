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

// Background services
builder.Services.AddHostedService<AgentHealthCheckService>();

// Swagger (API docs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Build Server API", Version = "v1" });
});

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
