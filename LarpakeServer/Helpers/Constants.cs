namespace LarpakeServer.Helpers;

public class Constants
{
    public const long NullId = -1;

    public const string CompletedStatusString = "completed";
    public const string PermissionsFieldName = "permissions";
    public const string StartYearFieldName = "start_year";

    // Localization
    public const string LangFin = "fi";
    public const string LangEng = "en";
    public const string LangDefault = LangFin;

    // LärpäkeEvent
    public const int MaxLarpakeTaskTitleLength = 80;
    public const int MaxLarpakeTaskBodyLength = 2000;
    public const int MaxPointsPerLarpakeTask = 100;

    // Lärpäke
    public const int MaxLarpakeTitleLength = 80;
    public const int MaxLarpakeDescriptionLength = 2000;

    // User
    public const int MaxUsernameLength = 255;

    public class Auth
    {
        public const string EntraIdScheme = "EntraIdBearer";
        public const string LarpakeIdScheme = "LarpakeIdBearer";
    }

    public class Luuppi
    {
        public const string BaseUrl = "https://luuppi.fi";
    }

    public class HttpClients
    {
        public const string IntegrationClient = "LuuppiClient";
    }
}


