using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class FreshmanGroupMemberPostDto
{
    [Required]
    public required Guid[] MemberIds { get; set; }
}
