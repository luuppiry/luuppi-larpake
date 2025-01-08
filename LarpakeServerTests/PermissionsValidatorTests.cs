using LarpakeServer.Helpers;
using LarpakeServer.Identity;

namespace LarpakeServerTests;

public class PermissionsValidatorTests
{
    [Theory]
    [InlineData(Permissions.None, Permissions.None)]    // None does not require any permissions

    [InlineData(Permissions.ManageFreshmanPermissions, Permissions.None)]
    [InlineData(Permissions.ManageFreshmanPermissions, Permissions.Freshman)]

    [InlineData(Permissions.ManageTutorPermissions, Permissions.None)]
    [InlineData(Permissions.ManageTutorPermissions, Permissions.Freshman)]
    [InlineData(Permissions.ManageTutorPermissions, Permissions.Freshman | Permissions.AddGroupMembers)]
    [InlineData(Permissions.ManageTutorPermissions, Permissions.Tutor)]
    [InlineData(Permissions.Admin, Permissions.Freshman)]
    [InlineData(Permissions.Admin, Permissions.Tutor)]
    [InlineData(Permissions.Tutor | Permissions.ManageTutorPermissions, Permissions.Tutor)]
    
    [InlineData(Permissions.ManageAdminPermissions, Permissions.Tutor)]
    [InlineData(Permissions.ManageAdminPermissions, Permissions.Tutor | Permissions.DeleteUser)]
    [InlineData(Permissions.ManageAdminPermissions, Permissions.Admin)]

    [InlineData(Permissions.ManageAllPermissions, (Permissions)int.MaxValue)]
    [InlineData(Permissions.Sudo, (Permissions)int.MaxValue)]
    public void IsAllowedToSet_ShouldReturnTrue_IfPermissionsAllow(Permissions author, Permissions target)
    {
        // Arrange & Act
        bool result = PermissionsValidator.IsAllowedToSet(author, target);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(Permissions.None, (Permissions)1)]      // Any permission
    [InlineData(Permissions.None, Permissions.Freshman)]  
    [InlineData(Permissions.Freshman, (Permissions)1)]  // Any permission
    [InlineData(Permissions.Tutor, (Permissions)1)]     // Any permission
    [InlineData(Permissions.ManageFreshmanPermissions, Permissions.Freshman + 1)]   // One higher from freshman
    [InlineData(Permissions.ManageTutorPermissions, Permissions.Tutor + 1)]   
    [InlineData(Permissions.Admin, Permissions.Tutor + 1)]   
    [InlineData(Permissions.Admin, Permissions.Sudo)]   
    [InlineData(Permissions.ManageAdminPermissions, Permissions.Admin + 1)]   
    public void IsAllowedToSet_ShouldReturnFalse_IfNoPermissions(Permissions author, Permissions target)
    {
        // Arrange & Act
        bool result = PermissionsValidator.IsAllowedToSet(author, target);
        
        // Assert
        Assert.False(result);
    }


    [Theory]
    [InlineData(Permissions.Admin, Permissions.Sudo)]    
    [InlineData(Permissions.Admin, Permissions.Admin)]    
    [InlineData(Permissions.Admin | Permissions.WriteAllData, Permissions.Admin)]   // Non full role  
    [InlineData(Permissions.Tutor, Permissions.Admin)]    
    [InlineData(Permissions.Tutor | Permissions.CreateEvent, Permissions.Tutor)]    // Non full role
    [InlineData(Permissions.Freshman | Permissions.CreateEvent, Permissions.Admin)]
    [InlineData(Permissions.Admin - 1, Permissions.Tutor)] // Non full role (still tutor)
    [InlineData(Permissions.Tutor - 1, Permissions.Freshman)] // Non full role (still freshman)
    [InlineData(Permissions.None, Permissions.None)]    
    public void HasHigherRoleOrSudo_ShouldReturnFalse_IfLowerRole(Permissions first, Permissions second)
    {
        // Arrange & Act
        bool result = PermissionsValidator.HasHigherRoleOrSudo(first, second);
        // Assert
        Assert.False(result);
    }
}
