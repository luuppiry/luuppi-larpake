using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class FreshmanGroupPostDto
{
    [Required]
    [MinLength(5)]
    [MaxLength(80)]
    public required string Name { get; set; }

    [Required]
    public required long LarpakeId { get; set; }

    public int? GroupNumber { get; set; } = null;



}
