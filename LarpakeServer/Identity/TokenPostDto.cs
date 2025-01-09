using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Identity;

public class TokenPostDto
{
    [Required]
    public required string AccessToken { get; init; }

    [Required]
    public required string RefreshToken { get; init; }

}
