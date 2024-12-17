using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class CompletedPutDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required long EventId { get; set; }

    public Guid? SignatureId { get; set; } = null;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
