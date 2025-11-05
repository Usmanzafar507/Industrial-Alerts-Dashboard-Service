using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Industrial.AlertService.Api.Controllers;

[ApiController]
[Route("alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertDto>>> Query(
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await _alertService.QueryAsync(status, from, to, ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/ack")]
    public async Task<ActionResult<AlertDto>> Acknowledge([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _alertService.AcknowledgeAsync(id, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}


