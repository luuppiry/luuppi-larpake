using LarpakeServer.Data.Helpers;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class GroupMemberIdCollection
{
    [MinLength(1)]
    [Required]
    public required Guid[] MemberIds { get; set; }
}

public class NonCompetingMemberIdCollection 
{
    [MinLength(1)]
    [Required]
    public required NonCompetingMember[] Members { get; set; }
}