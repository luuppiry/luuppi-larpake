namespace LarpakeServer.Models.External;

public class ExternalUserInformation
{
    public Guid EntraUserUuid { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
}
