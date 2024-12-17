using LarpakeServer.Identity;
using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class UserGetDto
{
    public required Guid Id { get; set; }
    public Permissions Permissions { get; set; }
    public int? StartYear { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal static UserGetDto From(User record)
    {
        return new UserGetDto
        {
            Id = record.Id,
            Permissions = record.Permissions,
            StartYear = record.StartYear,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt
        };
    }

}
