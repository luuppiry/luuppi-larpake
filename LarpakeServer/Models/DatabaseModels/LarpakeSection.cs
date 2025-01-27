using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeSection
{
    public required long Id { get; set; }
    public required long LarpakeId { get; set; }
    public int OrderingWeightNumber { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public required List<LarpakeSectionLocalization> TextData { get; set; }
    internal LarpakeSectionLocalization DefaultLocalization => GetDefaultLocalization();

    internal static LarpakeSection From(LarpakeSectionPostDto dto, long larpakeId)
    {
        return new LarpakeSection
        {
            Id = Constants.NullId,
            LarpakeId = larpakeId,
            TextData = dto.TextData.ToList(),
            OrderingWeightNumber = dto.OrderingWeightNumber,
        };
    }

    internal static LarpakeSection From(LarpakeSectionPutDto dto, long larpakeId)
    {
        var section = From((LarpakeSectionPostDto)dto, larpakeId);
        section.Id = dto.Id;
        return section;
    }

    private LarpakeSectionLocalization GetDefaultLocalization()
    {
        if (TextData is null || TextData.Count is 0)
        {
            throw new InvalidOperationException("No localization data found.");
        }
        return TextData.FirstOrDefault(
            x => x.LanguageCode == Constants.LangDefault, TextData.First());
    }
}
