using Industrial.AlertService.Domain.Entities;
using Industrial.AlertService.Domain.Interfaces;
using Industrial.AlertService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Industrial.AlertService.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AlertDbContext _db;

    public AlertRepository(AlertDbContext db)
    {
        _db = db;
    }

    public async Task<Alert> AddAsync(Alert alert, CancellationToken ct = default)
    {
        alert.Id = alert.Id == Guid.Empty ? Guid.NewGuid() : alert.Id;
        alert.CreatedAt = DateTime.UtcNow;
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync(ct);
        return alert;
    }

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<Alert?> AcknowledgeAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (alert == null) return null;
        alert.Status = "Acknowledged";
        await _db.SaveChangesAsync(ct);
        return alert;
    }

    public async Task<IReadOnlyList<Alert>> QueryAsync(string? status, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        IQueryable<Alert> q = _db.Alerts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("open", StringComparison.OrdinalIgnoreCase))
                q = q.Where(a => a.Status == "Open");
            else if (status.Equals("ack", StringComparison.OrdinalIgnoreCase) || status.Equals("acknowledged", StringComparison.OrdinalIgnoreCase))
                q = q.Where(a => a.Status == "Acknowledged");
        }
        if (from.HasValue) q = q.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(a => a.CreatedAt <= to.Value);
        q = q.OrderByDescending(a => a.CreatedAt);
        return await q.ToListAsync(ct);
    }
}


