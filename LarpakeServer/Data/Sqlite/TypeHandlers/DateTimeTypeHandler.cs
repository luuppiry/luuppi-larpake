using System.Data;

namespace LarpakeServer.Data.Sqlite.TypeHandlers;

public class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        if (value is DateTime dt)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
        if (value is string s)
        {
            return DateTime.SpecifyKind(DateTime.Parse(s), DateTimeKind.Utc);
        }
        throw new NotImplementedException($"No converter implemented for {value.GetType()}.");
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
