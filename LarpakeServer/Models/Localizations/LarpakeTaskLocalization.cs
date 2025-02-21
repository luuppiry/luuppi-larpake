using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class LarpakeTaskLocalization : LocalizationBase
{
    [Required]
    [Length(5, Constants.MaxLarpakeTaskTitleLength)]
    public required string Title { get; set; }

    [MaxLength(Constants.MaxLarpakeTaskBodyLength)]
    public string? Body { get; set; }
}
