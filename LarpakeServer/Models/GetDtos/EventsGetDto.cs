using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos;

public class EventsGetDto : GetDtoBase
{
    public required EventGetDto[] Events { get; set; }

    [JsonIgnore]
    public override int ItemCount => Events.Length;

    internal static EventsGetDto MapFrom(Event[] events)
    {
        return new EventsGetDto
        {
            Events = events.Select(EventGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
