using LarpakeServer.Models.PostDtos;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class LarpakeSectionPutDto : LarpakeSectionPostDto
{
    [Required]
    public required long Id { get; set; }
}