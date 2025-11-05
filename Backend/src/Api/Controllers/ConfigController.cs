using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Industrial.AlertService.Api.Controllers;

[ApiController]
[Route("config")]
[Authorize]
public class ConfigController : ControllerBase
{
    private readonly IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<ActionResult<ConfigDto>> Get(CancellationToken ct)
    {
        var cfg = await _configService.GetAsync(ct);
        return Ok(cfg);
    }

    [HttpPut]
    public async Task<ActionResult<ConfigDto>> Update([FromBody] UpdateConfigRequest request, CancellationToken ct)
    {
        try
        {
            var updated = await _configService.UpdateAsync(request, ct);
            return Ok(updated);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ValidationProblem(new ValidationProblemDetails
            {
                Title = "Validation errors",
                Status = StatusCodes.Status400BadRequest,
                Errors = new Dictionary<string, string[]> { { ex.ParamName ?? "param", new[] { ex.Message } } }
            });
        }
    }
}


