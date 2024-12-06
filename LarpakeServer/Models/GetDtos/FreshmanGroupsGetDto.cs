namespace LarpakeServer.Models.GetDtos;

public class FreshmanGroupsGetDto
{
    public required FreshmanGroupGetDto[] Groups { get; set; }
    public int NextPage { get; set; } = -1;

}
