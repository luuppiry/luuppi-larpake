using LarpakeServer.Models.DatabaseModels.Metadata;
using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.PostDtos;
using LarpakeServer.Models.PutDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class LarpakeTask : ILocalized<LarpakeTaskLocalization>
{
    public required long Id { get; set; }
    public required long LarpakeSectionId { get; set; }
    public required int Points { get; set; }
    public int OrderingWeightNumber { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public required List<LarpakeTaskLocalization> TextData { get; set; }
    internal LarpakeTaskLocalization DefaultLocalization => GetDefaultLocalization();


    internal static LarpakeTask From(LarpakeTaskPostDto record)
    {
        return new LarpakeTask
        {
            Id = Constants.NullId,
            LarpakeSectionId = record.LarpakeSectionId,
            TextData = record.TextData.ToList(),
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
        };
    }

    internal static LarpakeTask From(LarpakeTaskPutDto record)
    {
        return new LarpakeTask
        {
            Id = Constants.NullId,
            LarpakeSectionId = Constants.NullId,
            TextData = record.TextData.ToList(),
            Points = record.Points,
            OrderingWeightNumber = record.OrderingWeightNumber,
        };
    }

    private LarpakeTaskLocalization GetDefaultLocalization()
    {
        if (TextData is null || TextData.Count is 0)
        {
            throw new InvalidOperationException("No localization data found.");
        }
        return TextData.FirstOrDefault(
            x => x.LanguageCode == Constants.LangDefault, TextData.First());
    }
}
