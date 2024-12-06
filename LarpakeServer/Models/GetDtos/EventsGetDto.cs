using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class EventsGetDto
{
    public required EventGetDto[] Events { get; set; }

    public int NextPage { get; set; } = -1;


    internal static EventsGetDto MapFrom(Event[] events)
    {
        return new EventsGetDto
        {
            Events = events.Select(EventGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
