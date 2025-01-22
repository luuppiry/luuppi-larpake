using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeEventPostDto
{
    [Required]
    public required long LarpakeSectionId { get; set; }

    [Required]
    [Length(5, 80)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public string? Body { get; set; }

    [Required]
    public required int Points { get; set; } = 1;

    public int OrderingWeightNumber { get; set; } = 0;
}
