using Industrial.AlertService.Domain.DTOs;
using Industrial.AlertService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Industrial.AlertService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwt;

    public AuthController(IJwtTokenService jwt)
    {
        _jwt = jwt;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password required" });

        // Demo user validation
        if (request.Username == "demo" && request.Password == "Password123!")
        {
            var token = _jwt.GenerateToken("demo");
            return Ok(new LoginResponse(token));
        }
        return Unauthorized(new { message = "Invalid credentials" });
    }
}


