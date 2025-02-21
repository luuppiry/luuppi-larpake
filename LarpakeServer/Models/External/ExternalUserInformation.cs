using System.Text.Json.Serialization;

namespace LarpakeServer.Models.External;

public class ExternalUserInformation
{
    [JsonPropertyName("entraUserUuid")]
    public Guid EntraId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
}
