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
            Event current = events[i];
            result.Events[i] = new EventGetDto
            {
                Id = current.Id,
                Title = current.Title,
                Body = current.Body,
                StartTimeUtc = current.StartTimeUtc,
                EndTimeUtc = current.EndTimeUtc,
                Location = current.Location,
                WebsiteUrl = current.WebsiteUrl,
                ImageUrl = current.ImageUrl,
                SoftDeletionInfo = current.TimeDeletedUtc.HasValue
                    ? new SoftDeletionInfo(current.TimeDeletedUtc.Value) : null
            };
        }
        return result;




    }
}
