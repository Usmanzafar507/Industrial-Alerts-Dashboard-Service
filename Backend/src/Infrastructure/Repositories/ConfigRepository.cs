using Industrial.AlertService.Domain.Entities;
using Industrial.AlertService.Domain.Interfaces;
using Industrial.AlertService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Industrial.AlertService.Infrastructure.Repositories;

public class ConfigRepository : IConfigRepository
{
    private readonly AlertDbContext _db;

    public ConfigRepository(AlertDbContext db)
    {
        _db = db;
    }

    public async Task<Config> GetAsync(CancellationToken ct = default)
    {
        var cfg = await _db.Configs.AsNoTracking().FirstOrDefaultAsync(ct);
        if (cfg == null)
        {
            cfg = new Config
            {
                Id = Guid.NewGuid(),
                TempMax = 75.5m,
                HumidityMax = 60m,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Configs.Add(cfg);
            await _db.SaveChangesAsync(ct);
        }
        return cfg;
    }

    public async Task<Config> UpsertAsync(Config config, CancellationToken ct = default)
    {
        var existing = await _db.Configs.FirstOrDefaultAsync(ct);
        if (existing == null)
        {
            config.Id = config.Id == Guid.Empty ? Guid.NewGuid() : config.Id;
            _db.Configs.Add(config);
        }
        else
        {
            existing.TempMax = config.TempMax;
            existing.HumidityMax = config.HumidityMax;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        return existing ?? config;
    }
}


