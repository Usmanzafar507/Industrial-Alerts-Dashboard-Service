namespace Industrial.AlertService.Domain.DTOs;

public record ConfigDto(Guid Id, decimal TempMax, decimal HumidityMax, DateTime UpdatedAt);

public record UpdateConfigRequest(decimal TempMax, decimal HumidityMax);


