using LarpakeServer.Models.Localizations;

namespace LarpakeServer.Models.IdQueryObject;

public class EventIdObject : OrganizationEventLocalization
{
    public long OrganizationEventId { get; set; }
}
