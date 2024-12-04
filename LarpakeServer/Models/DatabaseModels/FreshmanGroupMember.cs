namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroupMember
{
    public required Guid UserId { get; set; }
    public required Guid FreshmanGroupId { get; set; }
    public DateTime DateJoinedUtc { get; set; }
}
