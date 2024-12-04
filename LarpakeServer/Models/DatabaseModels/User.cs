namespace LarpakeServer.Models.DatabaseModels;

public class User
{
    public required Guid Id { get; set; }
    public Permissions Persmissions { get; set; } = Permissions.None;
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
}
