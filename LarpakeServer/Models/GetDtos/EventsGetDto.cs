using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class EventsGetDto
{
    public required EventGetDto[] Events { get; set; }

    public int NextPage { get; set; } = -1;


    internal static EventsGetDto MapFrom(Event[] events)
    {
        var result = new EventsGetDto
        {
            Events = new EventGetDto[events.LongLength]
        };

        for (var i = 0; i < events.LongLength; i++)
        {
            result.Events[i] = EventGetDto.From(events[i]);
        }
        return result;
    }
}
