namespace LarpakeServer.Models.EventModels;

public readonly struct AttendedCreated 
{
    public required Guid UserId { get; init; }
    public required long LarpakeEventId { get; init; }
    public required Guid CompletionId { get; init; }
}
