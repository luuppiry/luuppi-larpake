using LarpakeServer.Models.Localizations;
using System.ComponentModel.DataAnnotations;

namespace LarpakeServer.Models.PutDtos;

public class LarpakeTaskPutDto
{

    [Required]
    [MinLength(1)]
    public required LarpakeTaskLocalization[] TextData { get; set; }

    [Required]
    [Range(1, Constants.MaxPointsPerLarpakeTask)]
    public required int Points { get; set; }

    public int OrderingWeightNumber { get; set; }

}
