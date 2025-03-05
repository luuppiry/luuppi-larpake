namespace LarpakeServer.Models.DatabaseModels;

public class FreshmanGroupMember
{
    public required Guid UserId { get; set; }
    public required long GroupId { get; set; }
    public bool IsHidden { get; set; } = false;
    public bool IsCompeting { get; set; } = true;
    public DateTime JoinedAt { get; set; }
}
