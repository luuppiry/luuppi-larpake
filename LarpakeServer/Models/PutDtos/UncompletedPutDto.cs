using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class UncompletedPutDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required long EventId { get; set; }
}
