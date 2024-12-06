using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class UserPutDto
{
    [Required]
    public int StartYear { get; set; } = -1;
}
