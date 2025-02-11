namespace LarpakeServer.Data.Helpers;

public readonly record struct NonCompetingMember(Guid UserId, bool IsHidden)
{
    public NonCompetingMember(Guid userId) : this(userId, false) { }
}