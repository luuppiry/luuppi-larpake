using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.Localizations;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.External;

public class ExternalEvent
{
    public long Id { get; set; }
    public string NameFi { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionFi { get; set; }
    public string? DescriptionEn { get; set; }
    public string? LocationFi { get; set; }
    public string? LocationEn { get; set; }

    [JsonPropertyName("start")]
    public DateTime StartsAt { get; set; }

    [JsonPropertyName("end")]
    public DateTime? EndsAt { get; set; }

    public bool HasTickets { get; set; }
    public string? ImageFiUrl { get; set; }
    public string? ImageEnUrl { get; set; }




    public OrganizationEvent ToOrganizationEvent()
    {
        // Example website url: "https://luuppi.fi/fi/events/227"
        return new OrganizationEvent
        {
            Id = Constants.NullId,
            StartsAt = StartsAt,
            EndsAt = EndsAt,
            ExternalId = $"{Constants.Luuppi.EventHeader}-{Id}",
            TextData = [
                new OrganizationEventLocalization
                {
                    LanguageCode = Constants.LangFin,
                    Location = LocationFi ?? "TBA",
                    Title = NameFi,
                    Body = DescriptionFi ?? string.Empty,
                    ImageUrl = ImageFiUrl,
                    WebsiteUrl = $"{Constants.Luuppi.BaseUrl}/fi/events/{Id}"
                },
                new OrganizationEventLocalization
                {
                    LanguageCode = Constants.LangEng,
                    Location = LocationEn ?? "TBA",
                    Title = NameEn,
                    Body = DescriptionEn ?? string.Empty,
                    ImageUrl = ImageEnUrl,
                    WebsiteUrl = $"{Constants.Luuppi.BaseUrl}/en/events/{Id}"
                }
            ]
        };
    }
}
