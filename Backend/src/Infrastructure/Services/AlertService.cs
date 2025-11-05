using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Entities;
using Industrial.AlertService.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Industrial.AlertService.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IHubContext<AlertsHub, IAlertsClient> _hubContext;

    public AlertService(IAlertRepository alertRepository, IHubContext<AlertsHub, IAlertsClient> hubContext)
    {
        _alertRepository = alertRepository;
        _hubContext = hubContext;
    }

    public async Task<AlertDto> CreateAlertAsync(string type, decimal value, decimal threshold, CancellationToken ct = default)
    {
        var alert = new Alert
        {
            Type = type,
            Value = value,
            Threshold = threshold,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };
        alert = await _alertRepository.AddAsync(alert, ct);
        var dto = new AlertDto(alert.Id, alert.Type, alert.Value, alert.Threshold, DateTime.SpecifyKind(alert.CreatedAt, DateTimeKind.Utc),alert.Status);
        await _hubContext.Clients.All.NewAlert(dto);
        return dto;
    }

    public async Task<IReadOnlyList<AlertDto>> QueryAsync(string? status, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var alerts = await _alertRepository.QueryAsync(status, from, to, ct);
        return alerts.Select(a => new AlertDto(a.Id, a.Type, a.Value, a.Threshold, DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc), a.Status)).ToList();
    }

    public async Task<AlertDto?> AcknowledgeAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _alertRepository.AcknowledgeAsync(id, ct);
        return alert == null ? null : new AlertDto(alert.Id, alert.Type, alert.Value, alert.Threshold, alert.CreatedAt, alert.Status);
    }
}

// SignalR hub contracts (interface for strong-typed hub)
public interface IAlertsClient
{
    Task NewAlert(AlertDto alert);
}

public class AlertsHub : Hub<IAlertsClient> { }


