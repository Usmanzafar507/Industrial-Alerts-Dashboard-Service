using System.ComponentModel.DataAnnotations;

namespace Industrial.AlertService.Domain.Entities;

public class Alert
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty; // "Temperature" or "Humidity"

    public decimal Value { get; set; }

    public decimal Threshold { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string Status { get; set; } = "Open"; // "Open" or "Acknowledged"
}


