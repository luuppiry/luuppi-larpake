using LarpakeServer.Controllers;
using SQLitePCL;

namespace LarpakeServer.Helpers;

public class Constants
{
    public const long NullId = -1;

    public const string CompletedStatusString = "completed";
    public const string PermissionsFieldName = "permissions";
    public const string StartYearFieldName = "start_year";

    // Url
    public const int MaxUrlLength = 150;

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
        public const string EventHeader = "lupev1";   // Luuppi event version 1
        public const string CmsApi = "https://cms.luuppi.fi";
        public const string Issuer = "larpake.luuppi.fi";
        public const string Audience = "larpake.luuppi.fi";
    }

    public class HttpClients
    {
        public const string IntegrationClient = "LuuppiClient";
    }

    public class Environment
    {
        public const string EntraClientId = "ENTRA_CLIENT_ID";
        public const string EntraTenantId = "ENTRA_TEDANT_ID";
        public const string LuuppiApiKey = "LUUPPI_API_KEY";
        public const string LarpakeIdSecret = "JWT_SECRET";
        public const string LarpakeIdIssuer = "JWT_ISSUER";
        public const string LarpakeIdAudience = "JWT_AUDIENCE";
        public const string PostgresConnectionString = "POSTGRES_CONNECTION_STRING";
        public const string EntraSudoUsers = "ENTRA_SUDO_USERS";
        public const string EnvVariablePrefix = "LARPAKE_";
    }

    public class Api
    {
        static string? _version;

        public static string Version
        {
            get
            {
                _version ??= GetVersion();
                return _version;
            }
        }

        public const string AppName = "Lärpäke Server";

        public const string Authors = "Henri Vainio";

        public const string Copyright = "Copyright (c) 2025 Henri Vainio";

        public const string Mode =
#if RELEASE
            "release";
#else
            "debug";
#endif


        private static string GetVersion()
        {
            return typeof(StatusController).Assembly.GetName().Version?.ToString() ?? "unknown";
        }
    }
}


