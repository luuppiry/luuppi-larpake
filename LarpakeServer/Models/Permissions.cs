namespace LarpakeServer.Models;

/// <summary>
/// Enum flags representing different permissions.
/// Normally use <see cref="Permissions.User"/>,
/// <see cref="Permissions.Tutor"/> or <see cref="Permissions.Admin"/>.
/// <see cref="Permissions.Sudo"/> is only for special occasions.
/// </summary>
[Flags]
public enum Permissions : int
{
    None = 0,

    #region USER_PERMISSIONS
    /// <summary>
    /// Read data from own year and group.
    /// </summary>
    CommonRead = 1 << 1,

    /// <summary>
    /// User can attend Lärpäke events.
    /// </summary>
    AttendEvent = 1 << 2,

    #endregion USER_PERMISSIONS
    #region TUTOR_PERMISSIONS

    /// <summary>
    /// User can set attendance as completed 
    /// with their own information.
    /// </summary>
    CompleteAttendance = 1 << 3,

    /// <summary>
    /// User can add signature to app database.
    /// </summary>
    AddSignature = 1 << 4,

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

    #endregion ADMIN_PERMISSIONS
    #region SUDO_PERMISSIONS

    // Sudo permissions are only used for special occasions. 
    // They should not be given to anyone other than developers.

    /// <summary>
    /// User can uncomplete or edit attendance record.
    /// </summary>
    EditAttendance = 1 << 27,

    /// <summary>
    /// Permission allows to read any data.
    /// </summary>
    ReadAllData = 1 << 28,

    /// <summary>
    /// Permission allows to write any data.
    /// </summary>
    WriteAllData = 1 << 29,

    /// <summary>
    /// Permission allows to hard delete any event.
    /// </summary>
    HardDeleteEvent = 1 << 30,

    #endregion SUDO_PERMISSIONS
    
    
    
    /* YOU SHOULD NEVER SHIFT OVER 30 (with int)
     * (Sign bit is 1 << 31 so we will also not touch it for now) 
     */


    // Combined permissions

    /// <summary>
    /// Permission allows to read common information and attend event.
    /// </summary>
    User = CommonRead | AttendEvent,

    /// <summary>
    /// Permission allow to complete attendance, 
    /// add group members and create group events.
    /// Also has all the permissions of User.
    /// </summary>
    Tutor = User | CompleteAttendance | AddSignature
        | AddGroupMembers | CreateGroupEvent,

    /// <summary>
    /// Permission allows to create, update and delete any event
    /// and create fuksi groups.
    /// Also has all the permissions of Tutor.
    /// </summary>
    Admin = Tutor | CreateGroup | CreateEvent | DeleteEvent
        | SeeHiddenMembers | EditAttendance | DeleteAttendance,

    /// <summary>
    /// This is a special permission that allows should
    /// not be given to anyone other than developers
    /// </summary>
    Sudo = int.MaxValue
}
