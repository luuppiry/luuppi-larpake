using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeEventPostDto
{
    [Required]
    public required long LarpakeSectionId { get; set; }

    [Required]
    [Length(5, Constants.MaxLarpakeEventTitleLength)]
    public required string Title { get; set; }

    [MaxLength(Constants.MaxLarpakeEventBodyLength)]
    public string? Body { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeEvent)]
    public required int Points { get; set; } = 1;

    public int OrderingWeightNumber { get; set; } = 0;
}
