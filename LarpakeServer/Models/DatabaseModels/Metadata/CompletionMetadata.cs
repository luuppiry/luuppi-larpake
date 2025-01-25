using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels.Metadata;

public class CompletionMetadata 
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }
    public required Guid SignerId { get; set; }
    public required DateTime CompletedAt { get; set; }


    public static CompletionMetadata From(CompletionPutDto dto, Guid signerId)
    {
        return new CompletionMetadata
        {
            Id = Guid.Empty,
            UserId = dto.UserId,
            EventId = dto.EventId,
            SignerId = signerId,
            CompletedAt = dto.CompletedAt,
        };
    }
}
