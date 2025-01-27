using LarpakeServer.Models.Localizations;
using LarpakeServer.Models.PostDtos;

namespace LarpakeServer.Models.DatabaseModels;

public class Larpake
{
    public required long Id { get; set; }
    public int? Year { get; set; } = null;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LarpakeSection>? Sections { get; set; }
    public required List<LarpakeLocalization> TextData { get; set; }
    internal LarpakeLocalization DefaultLocalization => GetDefaultLocalization();

    internal static Larpake From(LarpakePostDto record)
    {
        return new Larpake
        {
            Id = Constants.NullId,
            TextData = record.TextData.ToList(),
            Year = record.Year,
        };
    }

    private LarpakeLocalization GetDefaultLocalization()
    {
        if (TextData is null || TextData.Count is 0)
        {
            throw new InvalidOperationException("No localization data found.");
        }
        return TextData.FirstOrDefault(
            x => x.LanguageCode == Constants.LangDefault, TextData.First());
    }
}
