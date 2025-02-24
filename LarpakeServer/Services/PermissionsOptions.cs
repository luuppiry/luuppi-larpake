using LarpakeServer.Identity;

namespace LarpakeServer.Services;

public class PermissionsOptions
{
    public const string SectionName = "Permissions";

    public bool SetOnStartup { get; set; } = true;
    public bool FailIfNotFound { get; set; } = true;

    /// <summary>
    /// Application specific ids (not Entra ids).
    /// </summary>
    public UserGroup App { get; set; } = new();
    public Guid[] EntraSudoModeUsers { get; set; } = [];

    public int GetTargetCount()
    {
        return App.GetCount() + EntraSudoModeUsers.Length;
    }

    public class UserGroup
    {
        public Guid[] Sudo { get; set; } = [];
        public Guid[] Admin { get; set; } = [];
        public (Guid UserId, Permissions Permissions)[] Special { get; set; } = [];

        public int GetCount()
        {
            return Sudo.Length + Admin.Length + Special.Length;
        }
    }

    /// <summary>
    /// Parse a string of semicolon separated guids.
    /// Set the parsed guids as entra sudo users.
    /// </summary>
    /// <param name="semicolonSeparatedGuids">Example: 0194ad9a-c487-705e-90c0-2a046022a0c0;0194ad9a-c487-705e-90c0-2a046022a0c0;0194ad9a-c487-705e-90c0-2a046022a0c0</param>
    public void AddSudoUsersFromString(ReadOnlySpan<char> semicolonSeparatedGuids)
    {
        List<Guid> guids =[];
        foreach (Range span in semicolonSeparatedGuids.Split(';'))
        {
            Guid entraId = Guid.Parse(semicolonSeparatedGuids[span]);
            guids.Add(entraId);
        }
        EntraSudoModeUsers = guids.ToArray();
    }
}
