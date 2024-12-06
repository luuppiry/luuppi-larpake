using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class UsersGetDto
{
    public required UserGetDto[] Users { get; set; }

    public int NextPage { get; set; } = -1;

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
