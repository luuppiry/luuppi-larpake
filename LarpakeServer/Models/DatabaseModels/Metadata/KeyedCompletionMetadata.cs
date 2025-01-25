namespace LarpakeServer.Models.DatabaseModels.Metadata;

public class KeyedCompletionMetadata
{
    public required string Key { get; set; }
    public required Guid SignerId { get; set; }
    public required DateTime CompletedAt { get; set; }
}
