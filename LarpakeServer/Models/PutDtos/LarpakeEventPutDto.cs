using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class LarpakeEventPutDto
{

    [Required]
    [MinLength(1)]
    public required LarpakeEventLocalization[] TextData { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeEvent)]
    public required int Points { get; set; }

    public int OrderingWeightNumber { get; set; }

}
