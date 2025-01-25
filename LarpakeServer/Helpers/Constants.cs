﻿namespace LarpakeServer.Helpers;

public class Constants
{
    public const long NullId = -1;

    public const string CompletedStatusString = "completed";
    public const string PermissionsFieldName = "permissions";
    public const string StartYearFieldName = "start_year";

    #region LANGUAGE
    public const string LangFin = "fin";
    public const string LangEng = "eng";
    public const string LangDefault = LangFin;
    #endregion LANGUAGE


    #region LARPAKE_EVENT
    
    // LärpäkeEvent
    public const int MaxLarpakeEventTitleLength = 80;
    public const int MaxLarpakeEventBodyLength = 2000;
    public const int MaxPointsPerLarpakeEvent = 100;
    
    // Lärpäke
    public const int MaxLarpakeTitleLength = 80;
    public const int MaxLarpakeDescriptionLength = 2000;
    
    #endregion LARPAKE_EVENT

}
