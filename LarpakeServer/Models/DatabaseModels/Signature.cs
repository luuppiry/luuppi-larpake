namespace LarpakeServer.Models.DatabaseModels;

public class Signature
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string SignatureUrl { get; set; }
    public DateTime CreatedUtc { get; set; }
}
