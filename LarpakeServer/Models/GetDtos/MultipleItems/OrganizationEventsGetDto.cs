using LarpakeServer.Models.DatabaseModels;
using LarpakeServer.Models.GetDtos.SingleItem;
using LarpakeServer.Models.GetDtos.Templates;
using System.Text.Json.Serialization;

namespace LarpakeServer.Models.GetDtos.MultipleItems;

public class OrganizationEventsGetDto : GetDtoBase
{
    public required OrganizationEventGetDto[] Events { get; set; }

    [JsonIgnore]
    public override int ItemCount => Events.Length;

    internal static OrganizationEventsGetDto MapFrom(OrganizationEvent[] events)
    {
        return new OrganizationEventsGetDto
        {
            Events = events.Select(OrganizationEventGetDto.From).ToArray(),
            NextPage = -1
        };
    }
}
