namespace LarpakeServer.Models.EventModels;

public readonly struct AttendedCreated 
{
    public required Guid? TargetClientId { get; init; }
    public required Guid UserId { get; init; }
    public required long EventId { get; init; }
    public required Guid CompletionId { get; init; }
}
