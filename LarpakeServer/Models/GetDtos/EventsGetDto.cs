using LarpakeServer.Models.DatabaseModels;

namespace LarpakeServer.Models.GetDtos;

public class EventsGetDto : IPageable
{
    public required EventGetDto[] Events { get; set; }

    public int NextPage { get; set; } = -1;
    public int ItemCount => Events.Length;

    internal static EventsGetDto MapFrom(Event[] events)
    {
        return new EventsGetDto
        {
            Events = events.Select(EventGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
