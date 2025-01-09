using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class TokenPostDto
{
    [Required]
    public required string AccessToken { get; init; }

    [Required]
    public required string RefreshToken { get; init; }

}
