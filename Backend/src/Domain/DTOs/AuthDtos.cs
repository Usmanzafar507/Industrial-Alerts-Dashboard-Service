namespace Industrial.AlertService.Domain.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);


