using Industrial.AlertService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Industrial.AlertService.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(AlertDbContext db, ILogger logger, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        if (!await db.Configs.AnyAsync(ct))
        {
            db.Configs.Add(new Config
            {
                Id = Guid.NewGuid(),
                TempMax = 75.5m,
                HumidityMax = 60m,
                UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded default Config row.");
        }
    }
}


