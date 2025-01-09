namespace LarpakeServer.Identity;

/// <summary>
/// Enum flags representing different permissions.
/// Normally use <see cref="Freshman"/>,
/// <see cref="Tutor"/> or <see cref="Admin"/>.
/// <see cref="Sudo"/> is only for special occasions.
/// </summary>
[Flags]
public enum Permissions : int
{
    None = 0,

    #region FRESHMAN_PERMISSIONS

    /// <summary>
    /// Read data from own year and group.
    /// </summary>
    CommonRead = 1 << 1,

    /// <summary>
    /// User can attend Lärpäke events.
    /// </summary>
    AttendEvent = 1 << 2,

    #endregion FRESHMAN_PERMISSIONS
    #region TUTOR_PERMISSIONS

    /// <summary>
    /// User can set attendance as completed 
    /// with their own information.
    /// </summary>
    CompleteAttendance = 1 << 3,

    /// <summary>
    /// User can add signature to app database.
    /// </summary>
    CreateSignature = 1 << 4,

    /// <summary>
    /// User can add members to their own freshman group.
    /// </summary>
    AddGroupMembers = 1 << 5,

    /// <summary>
    /// User can create events for their own freshman group.
    /// </summary>
    CreateGroupEvent = 1 << 6,

    /// <summary>
    /// User can read common data from any year.
    /// </summary>
    ReadAnyYearData = 1 << 7,

    #endregion TUTOR_PERMISSIONS
    #region ADMIN_PERMISSIONS

    /// <summary>
    /// User can create new freshman groups.
    /// </summary>
    CreateGroup = 1 << 8,

    /// <summary>
    /// User can edit group information or members.
    /// </summary>
    EditGroup = 1 << 9,

    /// <summary>
    /// User can create new or edit events.
    /// </summary>
    CreateEvent = 1 << 10,

    /// <summary>
    /// User can delete events.
    /// </summary>
    DeleteEvent = 1 << 11,

    /// <summary>
    /// User can see hidden members in freshman groups.
    /// </summary>
    SeeHiddenMembers = 1 << 12,

    /// <summary>
    /// User can delete attendance records.
    /// </summary>
    DeleteAttendance = 1 << 13,

    /// <summary>
    /// See user information like ids and permissions.
    /// </summary>
    ReadRawUserInfomation = 1 << 14,

    /// <summary>
    /// Update user with lower role.
    /// Sudo -> Admin -> Tutor -> Freshman
    /// </summary>
    UpdateUserInformation = 1 << 15,

    /// <summary>
    /// User can delete user with lower role.
    /// </summary>
    DeleteUser = 1 << 16,

    /// <summary>
    /// User can uncomplete or edit attendance record.
    /// </summary>
    EditAttendance = 1 << 17,

    /// <summary>
    /// User can manage any permissions that user can have.
    /// </summary>
    ManageFreshmanPermissions = 1 << 18,

    /// <summary>
    /// User can manage any permissions that tutor can have.
    /// </summary>
    ManageTutorPermissions = 1 << 19 | ManageFreshmanPermissions,

    #endregion ADMIN_PERMISSIONS
    #region SUDO_PERMISSIONS

    // Sudo permissions are only used for special occasions. 
    // They should not be given to anyone other than developers.

    /// <summary>
    /// Permission allows to hard delete any event.
    /// </summary>
    HardDeleteEvent = 1 << 23,

    /// <summary>
    /// User can give or take admin permissions of any other admin 
    /// (if user has the permission).
    /// </summary>
    ManageAdminPermissions = 1 << 24 | ManageTutorPermissions,

    /// <summary>
    /// User can change any permission of any other user 
    /// (if user has the permission).
    /// </summary>
    ManageAllPermissions = 1 << 30 | ManageAdminPermissions,


    #endregion SUDO_PERMISSIONS



    /* YOU SHOULD NEVER SHIFT OVER 30 (with int)
     * (Sign bit is 1 << 31 so we will also not touch it for now) 
     */


    // Combined permissions

    /// <summary>
    /// Permission allows to read common information and attend event.
    /// </summary>
    Freshman = CommonRead | AttendEvent,

    /// <summary>
    /// Permission allow to complete attendance, 
    /// add group members and create group events.
    /// Also has all the permissions of User.
    /// </summary>
    Tutor = Freshman | CompleteAttendance | CreateSignature
        | AddGroupMembers | CreateGroupEvent,

    /// <summary>
    /// Permission allows to create, update and delete any event
    /// and create fuksi groups.
    /// Also has all the permissions of Tutor.
    /// </summary>
    Admin = Tutor | CreateGroup | CreateEvent | DeleteEvent
        | SeeHiddenMembers | EditAttendance | DeleteAttendance
        | ManageTutorPermissions | UpdateUserInformation | DeleteUser,

    /// <summary>
    /// This is a special permission that allows should
    /// not be given to anyone other than developers
    /// </summary>
    Sudo = int.MaxValue // All flags are set (but not the sign bit)
}
