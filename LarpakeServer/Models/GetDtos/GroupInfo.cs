namespace LarpakeServer.Models.GetDtos;

public class GroupInfo
{
    public long LarpakeId { get; set; }
    public required string Name { get; set; }
    public int GroupNumber { get; set; }
}
