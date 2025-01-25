using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.MultipleItems;

public class LarpakeEventsGetDto : GetDtoBase
{
    public required LarpakeEventGetDto[] Events { get; set; }

    [JsonIgnore]
    public override int ItemCount => Events.Length;

    internal static LarpakeEventsGetDto MapFrom(LarpakeEvent[] events)
    {
        return new LarpakeEventsGetDto
        {
            Events = events.Select(LarpakeEventGetDto.From).ToArray(),
        };
    }
}
