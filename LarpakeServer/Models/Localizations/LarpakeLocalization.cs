using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class LarpakeLocalization : LocalizationBase
{
    [Required]
    [Length(5, Constants.MaxLarpakeTitleLength)]
    public required string Title { get; set; }

    [MaxLength(Constants.MaxLarpakeDescriptionLength)]
    public required string Description { get; set; }
}
