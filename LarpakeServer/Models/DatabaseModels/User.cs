using LarpakeServer.Identity;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class User
{
    public required Guid Id { get; set; }
    public Guid? EntraId { get; set; }
    public string? EntraUsername { get; set; }
    public Permissions Permissions { get; set; } = Permissions.None;
    public int? StartYear { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static User MapFrom(UserPostDto dto, Guid? entraId = null)
    {
        return new User
        {
            Id = Guid.Empty,
            EntraId = entraId,
            Permissions = Permissions.None,
            StartYear = dto.StartYear,
        };
    }

    public static User MapFrom(UserPutDto dto)
    {
        return new User
        {
            Id = Guid.Empty,
            Permissions = Permissions.None,
            StartYear = dto.StartYear,
        };
    }

    public static User MapFrom(UserPermissionsPutDto dto)
    {
        return new User
        {
            Id = Guid.Empty,
            Permissions = dto.Permissions,
            StartYear = null,
        };
    }
}
