using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class UserPermissionsPutDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    public required Permissions Permissions { get; set; } 
}
