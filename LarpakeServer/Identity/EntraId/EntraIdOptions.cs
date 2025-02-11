namespace LarpakeServer.Identity.EntraId;

public class EntraIdOptions
{
    public const string SectionName = "EntraId";
    public Guid Instance { get; set; }
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
}
