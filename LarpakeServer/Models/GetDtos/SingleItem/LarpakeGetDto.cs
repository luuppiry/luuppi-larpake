using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos.SingleItem;

public class LarpakeGetDto : Larpake
{
    public static LarpakeGetDto From(Larpake larpake)
    {
        return new LarpakeGetDto
        {
            Id = larpake.Id,
            Title = larpake.Title,
            Year = larpake.Year,
            Description = larpake.Description,
            CreatedAt = larpake.CreatedAt,
            UpdatedAt = larpake.UpdatedAt,
            Sections = larpake.Sections,
        };
    }
}
