using System.ComponentModel.DataAnnotations;

namespace Industrial.AlertService.Domain.Entities;

public class Config
{
    [Key]
    public Guid Id { get; set; }

    [Range(0, 200)]
    public decimal TempMax { get; set; }

    [Range(0, 100)]
    public decimal HumidityMax { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


