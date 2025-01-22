using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels.Metadata;

public class CompletionMetadata : Completion
{
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }

    public static CompletionMetadata From(CompletionPutDto dto, Guid signerId)
    {
        return new CompletionMetadata
        {
            Id = Guid.Empty,
            UserId = dto.UserId,
            EventId = dto.EventId,
            SignerId = signerId,
            SignatureId = dto.SignatureId,
            CompletedAt = dto.CompletedAt,
        };
    }
}
