namespace LarpakeServer.Models.QueryOptions;

public class SignatureQueryOptions : QueryOptions
{

    public Guid? UserId { get; set; }

    public override bool HasNonNullValues()
    {
        return UserId is not null;
    }
}
