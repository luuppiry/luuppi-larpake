using LarpakeServer.Models.GetDtos;

namespace LarpakeServer.Models.Collections;

public class GroupMemberCollection
{
    public long GroupId { get; set; }
    public required UserGetDto[] Members { get; set; } = [];
    public required UserGetDto[] Tutors { get; set; } = [];
}
