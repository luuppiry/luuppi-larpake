using LarpakeServer.Models.ComplexDataTypes;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PostDtos;

public class SignaturePostDto
{
    [Required]
    public required Guid OwnerId { get; set; }

    [Required]
    public required Image Signature { get; set; }
}
