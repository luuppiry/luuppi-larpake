using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class LarpakeSectionLocalization : LocalizationBase
{
    [Required]
    [Length(5, Constants.MaxLarpakeTitleLength)]
    public required string Title { get; set; }
}
