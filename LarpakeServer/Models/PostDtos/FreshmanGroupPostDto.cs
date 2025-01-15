using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class FreshmanGroupPostDto
{
    [Required]
    [MinLength(5)]
    [MaxLength(80)]
    public required string Name { get; set; } 
    
    public int StartYear { get; set; } = -1;

    public int GroupNumber { get; set; } = -1;
}
