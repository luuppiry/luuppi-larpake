using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Models;

/// <summary>
/// Information about soft deletion of the entity.
/// If class is not null, the entity is soft deleted.
/// </summary>
public class SoftDeletionInfo
{
    public SoftDeletionInfo() { }

    [SetsRequiredMembers]
    public SoftDeletionInfo(DateTime timeDeletedUtc)
    {
        TimeDeletedUtc = timeDeletedUtc;
    }

    [SetsRequiredMembers]
    public SoftDeletionInfo(DateTime timeDeletedUtc, string reason) : this(timeDeletedUtc)
    {
        Reason = reason;
    }

    public readonly bool IsDeleted = true;
    public required DateTime TimeDeletedUtc { get; init; }
    public string Reason { get; init; } = "This item was deleted.";
}
