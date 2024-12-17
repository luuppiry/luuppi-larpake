using LarpakeServer.Identity;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class UserPermissionsPutDto
{
    [Required]
    public required Permissions Permissions { get; set; } 
}
