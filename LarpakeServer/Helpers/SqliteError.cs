namespace LarpakeServer.Helpers;

/// <summary>
/// Enum for SQLite error codes.
/// trailing _e is used to indicate code is extended error code.
/// </summary>
public class SqliteError
{
    public const int Constraint = 19;
    public const int ForeignKey_e = 787;
    public const int PrimaryKey_e = 1555;
}
