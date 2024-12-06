namespace LarpakeServer.Models;

public class LuuppiUser
{
    public required Guid LuuppiUserId { get; set; }
    public bool IsLuuppiHato { get; set; } = false;
    public bool IsLuuppiMember { get; set; } = false;
    public required string Username { get; set; }
}
