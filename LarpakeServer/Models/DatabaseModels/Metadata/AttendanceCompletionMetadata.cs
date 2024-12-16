using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels.Metadata;

public class AttendanceCompletionMetadata : AttendanceCompletion
{
    public required Guid UserId { get; set; }
    public required long EventId { get; set; }

    public static AttendanceCompletionMetadata From(CompletedPutDto dto, Guid signerId)
    {
        return new AttendanceCompletionMetadata
        {
            Id = Guid.Empty,
            UserId = dto.UserId,
            EventId = dto.EventId,
            SignerId = signerId,
            SignatureId = dto.SignatureId,
            CompletionTimeUtc = dto.CompletionDateUtc,
        };
    }
}
