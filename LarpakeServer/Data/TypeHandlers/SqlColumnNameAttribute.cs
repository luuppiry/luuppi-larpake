using System.Reflection;

namespace LarpakeServer.Data.TypeHandlers;

/// <summary>
/// Use reflection to create sql mapper to map columns to properties.
/// </summary>

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class SqlColumnNameAttribute : Attribute
{
    public string Name { get; }
    public SqlColumnNameAttribute(string name)
    {
        Name = name;
    }
}

public class ColumnAttributeTypeMapper<T> : FallbackTypeMapper
{
    public ColumnAttributeTypeMapper()
        : base([
            new CustomPropertyTypeMap(typeof(T), _propertySelector),
            new DefaultTypeMap(typeof(T))
        ])
    {
    }


    // Forgive nulls, property should exists in type the attreibute is used to
    static readonly Func<Type, string, PropertyInfo> _propertySelector = (type, columnName) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(
                    p => p.GetCustomAttributes(false)
                        .OfType<SqlColumnNameAttribute>()
                        .Any(attr => attr.Name == columnName))!;
}


public class FallbackTypeMapper : SqlMapper.ITypeMap
{
    private readonly IEnumerable<SqlMapper.ITypeMap> _mappers;

    public FallbackTypeMapper(IEnumerable<SqlMapper.ITypeMap> mappers)
    {
        _mappers = mappers;
    }

    public ConstructorInfo? FindConstructor(string[] names, Type[] types)
    {
        foreach (var mapper in _mappers)
        {
            try
            {
                ConstructorInfo? result = mapper.FindConstructor(names, types);
                if (result is not null)
                {
                    return result;
                }
            }
            catch
            {
            }
        }
        return null;
    }

    public ConstructorInfo? FindExplicitConstructor()
    {
        foreach (var mapper in _mappers)
        {
            try
            {
                ConstructorInfo? result = mapper.FindExplicitConstructor();
                if (result is not null)
                {
                    return result;
                }
            }
            catch
            {
            }
        }
        return null;
    }

    public SqlMapper.IMemberMap? GetConstructorParameter(ConstructorInfo constructor, string columnName)
    {
        foreach (var mapper in _mappers)
        {
            try
            {
                SqlMapper.IMemberMap? result = mapper.GetConstructorParameter(constructor, columnName);
                if (result is not null)
                {
                    return result;
                }
            }
            catch
            {
            }
        }
        return null;
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        foreach (var mapper in _mappers)
        {
            try
            {
                SqlMapper.IMemberMap? result = mapper.GetMember(columnName);
                if (result is not null)
                {
                    return result;
                }
            }
            catch
            {
            }
        }
        return null;
    }
}
