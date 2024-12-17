using LarpakeServer.Identity;
using System.Diagnostics.CodeAnalysis;

namespace LarpakeServer.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Checks if the <paramref name="permissions"/> has at least 
    /// the same flags as <paramref name="value"/>.
    /// </summary>
    /// <param name="permissions">Value representing someones permissions.</param>
    /// <param name="value">Value to check if exists in <paramref name="permissions"/></param>
    /// <returns>
    /// <see langword="true"/> if all true bits in <paramref name="value"/> 
    /// exists in <paramref name="permissions"/>, otherwise <see langword="false"/>.
    /// </returns>
    public static bool Has(this Permissions permissions, Permissions value)
    {
        return (permissions & value) == value;
    }

    public static bool TryConvertPermissions(string? value, [NotNullWhen(true)]out Permissions? result) 
    {
        if (int.TryParse(value, out int raw))
        {
            result = (Permissions)raw;
            return true;
        }
        result = null;
        return false;
    }

}
