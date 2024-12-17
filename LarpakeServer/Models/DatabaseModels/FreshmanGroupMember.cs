namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroupMember
{
    public required Guid UserId { get; set; }
    public required long FreshmanGroupId { get; set; }
    public DateTime JoinedAt { get; set; }
}
