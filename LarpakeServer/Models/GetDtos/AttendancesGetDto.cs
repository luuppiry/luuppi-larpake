using LarpakeServer.Models.DatabaseModels;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class AttendancesGetDto : IPageable
{
    public required AttendanceGetDto[] Attendances { get; set; }

    public int NextPage { get; set; } = -1;

    [JsonIgnore]
    public int ItemCount => Attendances.Length;

    public static AttendancesGetDto MapFrom(Attendance[] records)
    {
        return new AttendancesGetDto
        {
            Attendances = records.Select(AttendanceGetDto.From).ToArray()
        };
    }
}
