using LarpakeServer.Extensions;


namespace LarpakeServer.Identity;

internal static class PermissionsValidator
{
    /// <summary>
    /// Validates if <paramref name="author"/> has permissions to set permission <paramref name="targetValue"/>.
    /// </summary>
    /// <param name="author"></param>
    /// <param name="targetValue"></param>
    /// <returns>
    /// <see langword="true"/> if author's permissions allows setting <paramref name="targetValue"/>, otherwise <see langword="false"/>.</returns>
    /// <exception cref="NotImplementedException">If used dependency method returns invalid value.</exception>
    internal static bool IsAllowedToSet(this Permissions author, Permissions targetValue)
    {
        Permissions requiredRole = targetValue.GetLowestPartialRole();

        return requiredRole switch
        {
            Permissions.None => true,
            Permissions.Freshman => author.Has(Permissions.ManageFreshmanPermissions),
            Permissions.Tutor => author.Has(Permissions.ManageTutorPermissions),
            Permissions.Admin => author.Has(Permissions.ManageAdminPermissions),
            Permissions.Sudo => author.Has(Permissions.ManageAllPermissions),
            _ => throw new NotImplementedException($"Permission {requiredRole} not implemented."),
        };
    }


    /// <summary>
    /// Converts permissions to highest possible 
    /// role that user can act in with current permissions. 
    /// Then checks if <paramref name="first"/> has higher 
    /// role than <paramref name="second"/>.
    /// <para/>
    /// Role order is: 
    /// <see cref="Permissions.Sudo"/> >
    /// <see cref="Permissions.Admin"/> >
    /// <see cref="Permissions.Tutor"/> >
    /// <see cref="Permissions.Freshman"/> >
    /// <see cref="Permissions.None"/>
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="first"/> is <see cref="Permissions.Sudo"/> 
    /// OR <see langword="true"/> has one level higher role than second, 
    /// otherwise <see langword="false"/>.
    /// </returns>
    internal static bool HasHigherRoleOrSudo(this Permissions first, Permissions second)
    {
        Permissions firstRole = first.GetHighestFullRole();
        if (firstRole == Permissions.Sudo)
        {
            return true;
        }

        Permissions secondRole = second.GetHighestFullRole();
        return firstRole > secondRole;
    }




    private static Permissions GetLowestPartialRole(this Permissions permissions)
    {
        if (permissions is 0)
        {
            return Permissions.None;
        }

        // lookup order matters here lowest -> highest
        ReadOnlySpan<Permissions> roles =
        [
            Permissions.Freshman,
            Permissions.Tutor,
            Permissions.Admin,
        ];

        foreach (Permissions role in roles)
        {
            // Remove role bits and check if higher permissions are still present
            if ((permissions & ~role) is 0)
            {
                return role;
            }
        }
        return Permissions.Sudo;
    }

    private static Permissions GetHighestFullRole(this Permissions permissions)
    {
        // Lookup order matters here highest -> lowest
        ReadOnlySpan<Permissions> roles =
        [
            Permissions.Sudo,
            Permissions.Admin,
            Permissions.Tutor,
            Permissions.Freshman
        ];

        foreach (Permissions role in roles)
        {
            // Return first (highest) role that user has all permissions on
            if (permissions.Has(role))
            {
                return role;
            }
        }
        return Permissions.None;
    }
}
