using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class LarpakePostDto
{
    [Required]
    [Length(5, Constants.MaxLarpakeTitleLength)]
    public required string Title { get; set; }

    public int? Year { get; set; } = null;

    [MaxLength(Constants.MaxLarpakeDescriptionLength)]
    public required string Description { get; set; }
}
