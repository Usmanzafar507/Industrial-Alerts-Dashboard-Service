using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Entities;
using Industrial.AlertService.Domain.Interfaces;

namespace Industrial.AlertService.Infrastructure.Services;

public class ConfigService : IConfigService
{
    private readonly IConfigRepository _repository;

    public ConfigService(IConfigRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConfigDto> GetAsync(CancellationToken ct = default)
    {
        var cfg = await _repository.GetAsync(ct);
        return new ConfigDto(cfg.Id, cfg.TempMax, cfg.HumidityMax, cfg.UpdatedAt);
    }

    public async Task<ConfigDto> UpdateAsync(UpdateConfigRequest request, CancellationToken ct = default)
    {
        if (request.TempMax < 0 || request.TempMax > 200)
            throw new ArgumentOutOfRangeException(nameof(request.TempMax), "Temperature must be between 0 and 200.");
        if (request.HumidityMax < 0 || request.HumidityMax > 100)
            throw new ArgumentOutOfRangeException(nameof(request.HumidityMax), "Humidity must be between 0 and 100.");

        var current = await _repository.GetAsync(ct);
        var updated = new Config
        {
            Id = current.Id,
            TempMax = request.TempMax,
            HumidityMax = request.HumidityMax,
            UpdatedAt = DateTime.UtcNow
        };
        var saved = await _repository.UpsertAsync(updated, ct);
        return new ConfigDto(saved.Id, saved.TempMax, saved.HumidityMax, saved.UpdatedAt);
    }
}


