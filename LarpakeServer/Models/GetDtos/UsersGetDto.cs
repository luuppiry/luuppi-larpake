using LarpakeServer.Models.DatabaseModels;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class UsersGetDto : IPageable
{
    public required UserGetDto[] Users { get; set; }

    public int NextPage { get; set; } = -1;

    [JsonIgnore]
    public int ItemCount => Users.Length;

    public static UsersGetDto MapFrom(User[] records)
    {
        var dto = new UsersGetDto
        {
            Users = new UserGetDto[records.LongLength]
        };

        for (var i = 0; i < records.LongLength; i++)
        {
            dto.Users[i] = UserGetDto.From(records[i]);
        }
        return dto;
    }
}
