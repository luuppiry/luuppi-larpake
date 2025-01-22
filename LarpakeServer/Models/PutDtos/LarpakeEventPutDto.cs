using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class LarpakeEventPutDto
{
    [Required]
    [Length(5, Constants.MaxLarpakeEventTitleLength)]
    public required string Title { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeEvent)]
    public required int Points { get; set; }

    [MaxLength(Constants.MaxLarpakeEventBodyLength)]
    public string? Body { get; set; }

    public int OrderingWeightNumber { get; set; }

}
