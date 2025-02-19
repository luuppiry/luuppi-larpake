using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;

namespace LarpakeServer.Models.GetDtos;

public class UserGetDto : IMappable<User, UserGetDto>
{
    public required Guid Id { get; set; }
    public Guid? EntraId { get; set; }
    public string? EntraUsername { get; set; }
    public Permissions Permissions { get; set; }
    public int? StartYear { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static UserGetDto From(User record)
    {
        return new UserGetDto
        {
            Id = record.Id,
            EntraId = record.EntraId,
            EntraUsername = record.EntraUsername,
            Permissions = record.Permissions,
            StartYear = record.StartYear,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }

}
