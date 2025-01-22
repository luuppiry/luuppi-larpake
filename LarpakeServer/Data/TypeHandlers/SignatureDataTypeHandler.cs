using LarpakeServer.Models.ComplexDataTypes;
using System.Data;

namespace LarpakeServer.Data.TypeHandlers;

public class SignatureDataTypeHandler : SqlMapper.TypeHandler<List<List<UnsignedPoint2D>>>
{
    public override List<List<UnsignedPoint2D>>? Parse(object value)
    {
        throw new NotImplementedException();
    }

    public override void SetValue(IDbDataParameter parameter, List<List<UnsignedPoint2D>>? value)
    {
        throw new NotImplementedException();
    }
}
