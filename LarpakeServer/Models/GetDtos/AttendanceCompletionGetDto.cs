using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class AttendanceCompletionGetDto
{
    public required Guid Id { get; set; }
    public required Guid SignerId { get; set; }
    public Guid? SignatureId { get; set; } = null;
    public DateTime CompletionTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    internal static AttendanceCompletionGetDto From(AttendanceCompletion completion)
    {
        return new AttendanceCompletionGetDto
        {
            Id = completion.Id,
            SignerId = completion.SignerId,
            SignatureId = completion.SignatureId,
            CompletionTime = completion.CompletionTimeUtc
        };
    }
}
