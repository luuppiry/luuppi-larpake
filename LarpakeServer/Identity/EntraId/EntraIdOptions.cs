namespace LarpakeServer.Identity.EntraId;

public class EntraIdOptions
{
    public const string SectionName = "EntraId";
    public string? Instance { get; set; }
    public string? ClientId { get; set; }
    public string? TenantId { get; set; }
}
