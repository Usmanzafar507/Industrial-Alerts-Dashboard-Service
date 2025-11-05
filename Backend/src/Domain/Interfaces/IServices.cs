using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Entities;

namespace Industrial.AlertService.Domain.Interfaces;

public interface IThresholdEvaluator
{
    bool IsTemperatureExceeded(decimal value, decimal threshold);
    bool IsHumidityExceeded(decimal value, decimal threshold);
}

public interface IAlertService
{
    Task<AlertDto?> AcknowledgeAsync(Guid id, CancellationToken ct = default);
    Task<AlertDto> CreateAlertAsync(string type, decimal value, decimal threshold, CancellationToken ct = default);
    Task<IReadOnlyList<AlertDto>> QueryAsync(string? status, DateTime? from, DateTime? to, CancellationToken ct = default);
}

public interface IConfigService
{
    Task<ConfigDto> GetAsync(CancellationToken ct = default);
    Task<ConfigDto> UpdateAsync(UpdateConfigRequest request, CancellationToken ct = default);
}

public interface IJwtTokenService
{
    string GenerateToken(string username, IEnumerable<KeyValuePair<string, string>>? claims = null);
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}


