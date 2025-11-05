namespace Industrial.AlertService.Domain.DTOs;

public record AlertDto(Guid Id, string Type, decimal Value, decimal Threshold, DateTime CreatedAt, string Status);


