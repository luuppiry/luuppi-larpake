using System.Text.Json.Serialization;

namespace LarpakeServer.Models.External;

public class ExternalEvent
{
    public long Id { get; set; }
    public string NameFi{ get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionFi { get; set; }
    public string? DescriptionEn { get; set; }
    public string? LocationFi { get; set; }
    public string? LocationEn { get; set; }

    [JsonPropertyName("start")]
    public DateTime? StartsAt { get; set; }

    [JsonPropertyName("end")]
    public DateTime? EndsAt { get; set; }

    public bool HasTickets { get; set; }
    public string? ImageFiUrl { get; set; }
    public string? ImageEnUrl { get; set; }
}
