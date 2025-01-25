using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class CompletionPutDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required long EventId { get; set; }


    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
