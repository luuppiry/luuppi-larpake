using System.Data;

namespace LarpakeServer.Data.TypeHandlers;

public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }

    public override Guid Parse(object value)
    {
        if (value is Guid guid)
        {
            return guid;
        }
        if (value is string s)
        {
            return Guid.Parse(s);
        }
        throw new NotImplementedException("Only strings and guids can be converted to Guid.");
    }
}
