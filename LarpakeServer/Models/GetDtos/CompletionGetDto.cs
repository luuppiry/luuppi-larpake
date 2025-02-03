using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;

namespace LarpakeServer.Models.GetDtos;

public class CompletionGetDto : IMappable<Completion, CompletionGetDto>
{
    public required Guid Id { get; set; }
    public required Guid SignerId { get; set; }
    public Guid? SignatureId { get; set; } = null;
    public DateTime CompletionTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static CompletionGetDto From(Completion completion)
    {
        return new CompletionGetDto
        {
            Id = completion.Id,
            SignerId = completion.SignerId,
            SignatureId = completion.SignatureId,
            CompletionTime = completion.CompletedAt
        };
    }
}
