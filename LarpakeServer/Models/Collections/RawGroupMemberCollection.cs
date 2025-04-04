using LarpakeServer.Models.DatabaseModels;
using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Models.Collections;

public class RawGroupMemberCollection
{
    [SetsRequiredMembers]
    public RawGroupMemberCollection(long groupId)
    {
        GroupId = groupId;
    }

    public required long GroupId { get; set; }
    public List<Guid> Members { get; set; } = [];
    public List<Guid> Tutors { get; set; } = [];
}