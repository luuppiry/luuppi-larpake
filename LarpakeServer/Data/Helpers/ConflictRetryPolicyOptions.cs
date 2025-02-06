namespace LarpakeServer.Data.Helpers;

public class ConflictRetryPolicyOptions
{
    public const string SectionName = "ConflictRetryPolicy";
    public int MaxRetries { get; set; } = 1;
}
