using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class GroupMemberIdCollection
{
    [Required]
    public required Guid[] MemberIds { get; set; }
}
