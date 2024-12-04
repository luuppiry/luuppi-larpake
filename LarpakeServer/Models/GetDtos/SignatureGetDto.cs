namespace LarpakeServer.Models.GetDtos;

public class SignatureGetDto
{
    public required Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public required string SignatureUrl { get; set; }
}
