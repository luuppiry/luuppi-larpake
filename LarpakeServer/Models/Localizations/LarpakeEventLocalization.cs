using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class LarpakeEventLocalization : LocalizationBase
{
    [Required]
    [Length(5, Constants.MaxLarpakeEventTitleLength)]
    public required string Title { get; set; }

    [MaxLength(Constants.MaxLarpakeEventBodyLength)]
    public string? Body { get; set; }
}
