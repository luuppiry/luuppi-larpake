using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.Localizations;

public class LocalizationBase
{
    [Required]
    [AllowedValues("fi", "en", "default")]
    public required string LanguageCode
    {
        get => field;
        set => field = value is "default" ? Constants.LangDefault : value;
    } = Constants.LangDefault;
}
