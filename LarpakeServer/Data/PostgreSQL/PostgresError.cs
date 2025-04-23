namespace LarpakeServer.Data.PostgreSQL;

public class PostgresError
{
    public const string Success = "0";
    public const string UniqueViolation = "23505";
    public const string ForeignKeyViolation = "23503";
    public const string CheckConstraint = "23514";
    public const string NotNullConstraint = "23502";
}
