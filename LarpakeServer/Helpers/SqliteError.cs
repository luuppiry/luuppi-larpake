namespace LarpakeServer.Helpers;

/// <summary>
/// Enum for SQLite error codes.
/// trailing _e is used to indicate code is extended error code.
/// </summary>
public class SqliteError
{
    public const int Constraint = SQLitePCL.raw.SQLITE_CONSTRAINT;
    public const int ForeignKey_e = SQLitePCL.raw.SQLITE_CONSTRAINT_FOREIGNKEY;
    public const int PrimaryKey_e = SQLitePCL.raw.SQLITE_CONSTRAINT_PRIMARYKEY;
}
