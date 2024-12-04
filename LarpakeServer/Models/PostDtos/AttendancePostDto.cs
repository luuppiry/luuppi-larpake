using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class AttendancePostDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required Guid EventId { get; set; }
}
