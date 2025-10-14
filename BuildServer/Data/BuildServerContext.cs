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
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasIndex(a => a.AgentId).IsUnique();
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasIndex(j => j.JobId).IsUnique();
        });

        modelBuilder.Entity<AgentConfiguration>(entity =>
        {
            entity.HasIndex(ac => ac.AgentId).IsUnique();
        });
    }
}
