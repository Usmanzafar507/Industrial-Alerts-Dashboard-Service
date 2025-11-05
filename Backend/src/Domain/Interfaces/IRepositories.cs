using Industrial.AlertService.Domain.Entities;

namespace Industrial.AlertService.Domain.Interfaces;

public interface IConfigRepository
{
    Task<Config> GetAsync(CancellationToken ct = default);
    Task<Config> UpsertAsync(Config config, CancellationToken ct = default);
}

public interface IAlertRepository
{
    Task<Alert> AddAsync(Alert alert, CancellationToken ct = default);
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Alert?> AcknowledgeAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> QueryAsync(string? status, DateTime? from, DateTime? to, CancellationToken ct = default);
}


