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
            UserId = dto.UserId,
            EventId = dto.EventId,
            Id = Guid.Empty,
            SignerId = signerId,
            SignatureId = dto.SignatureId,
            CompletionTimeUtc = dto.CompletionDateUtc,
        };
    }
}
