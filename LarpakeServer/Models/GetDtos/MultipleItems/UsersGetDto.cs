using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.MultipleItems;

public class UsersGetDto : GetDtoBase
{
    public required UserGetDto[] Users { get; set; }

    [JsonIgnore]
    public override int ItemCount => Users.Length;

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
