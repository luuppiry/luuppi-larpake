namespace LarpakeServer.Data.Helpers;


internal class SelectQuery
{
    readonly StringBuilder _builder = new();
    bool _hasWhere = false;

    /// <summary>
    /// Same as <see cref="StringBuilder.AppendLine(string?)"/>
    /// </summary>
    /// <param name="line"></param>
    internal virtual SelectQuery AppendLine(string line)
    {
        _builder.AppendLine(line);
        return this;
    }

    /// <summary>
    /// Append a condition line to the query.
    /// Automatically chooses whether to append WHERE or AND if already conditions appended.
    /// </summary>
    /// <param name="condition"></param>
    internal virtual SelectQuery AppendConditionLine(string condition)
    {
        if (_hasWhere is false)
        {
            _builder.Append("WHERE ");
            _hasWhere = true;
        }
        else
        {
            _builder.Append("AND ");
        }
        _builder.AppendLine(condition);
        return this;
    }
    internal string Build()
    {
        return _builder.ToString();
    }

    public override string ToString() => Build();



    class NullQuery : SelectQuery
    {
        /* These functions should not do anything */

        internal override NullQuery AppendConditionLine(string condition)
        {
            return this;
        }

        internal override NullQuery AppendLine(string line)
        {
            return this;
        }
    }

    public SelectQuery IfTrue(bool? condition)
    {
        return If(condition is true);
    }
    
    public SelectQuery IfFalse(bool? condition)
    {
        return If(condition is false);
    }

    public SelectQuery If(bool condition)
    {
        return condition ? this : new NullQuery();
    }

    public SelectQuery IfNot(bool condition)
    {
        return If(condition is false);
    }

    public SelectQuery IfNull<T>(T? value)
    {
        return value is null ? this : new NullQuery();
    }
    public SelectQuery IfNotNull<T>(T? value)
    {
        return value is not null ? this : new NullQuery();
    }
}