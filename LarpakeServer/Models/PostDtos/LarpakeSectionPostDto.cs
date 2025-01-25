using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakeSectionPostDto
{
    [Required]
    [Length(5, Constants.MaxLarpakeTitleLength)]
    public required string Title { get; set; }

    public int OrderingWeightNumber { get; set; } = 0;
}