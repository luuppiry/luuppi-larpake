using LarpakeServer.Identity;

namespace LarpakeServer.Services;

public class PermissionsOptions
{
    public const string SectionName = "Permissions";

    public bool SetOnStartup { get; set; } = true;
    public bool FailIfNotFound { get; set; } = true;

    public Guid[] Sudo { get; set; } = [];
    public Guid[] Admin { get; set; } = [];
    public (Guid UserId, Permissions Permissions)[] Special { get; set; } = [];


    public int GetTargetCount()
    {
        return Sudo.Length + Admin.Length + Special.Length;
    }
}
