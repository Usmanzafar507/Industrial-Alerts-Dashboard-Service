using Industrial.AlertService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Industrial.AlertService.Infrastructure.Persistence;

public class AlertDbContext : DbContext
{
    public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options) {}

    public DbSet<Config> Configs => Set<Config>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Config>(entity =>
        {
            entity.ToTable("Config");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TempMax).HasColumnType("decimal(18,2)");
            entity.Property(x => x.HumidityMax).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alert");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Value).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Threshold).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).IsRequired();
        });
    }
}


