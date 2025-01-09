using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class AttendancesGetDto : GetDtoBase
{
    public required AttendanceGetDto[] Attendances { get; set; }

    [JsonIgnore]
    public override int ItemCount => Attendances.Length;

    public static AttendancesGetDto MapFrom(Attendance[] records)
    {
        return new AttendancesGetDto
        {
            Attendances = records.Select(AttendanceGetDto.From).ToArray()
        };
    }
}
