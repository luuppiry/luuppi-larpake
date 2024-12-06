namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupGetDto
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public int StartYear { get; set; } = -1;
    public int GroupNumber { get; set; } = -1;
    public Guid[] MemberIds { get; set; } = [];
}
