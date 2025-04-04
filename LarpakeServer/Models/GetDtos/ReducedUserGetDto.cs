using LarpakeServer.Identity;

namespace LarpakeServer.Models.GetDtos;

public class ReducedUserGetDto
{
    public required Guid Id { get; set; }
    public string? Username { get; set; }
    public Permissions Permissions { get; set; }
    public int? StartYear { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static ReducedUserGetDto From(UserGetDto user)
    {
        return new ReducedUserGetDto
        {
            Id = user.Id,
            Username = user.Username,
            Permissions = user.Permissions,
            StartYear = user.StartYear,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
