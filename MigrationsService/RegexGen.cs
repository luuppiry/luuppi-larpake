using System.Text.RegularExpressions;

namespace MigrationsService;
internal partial class RegexGen
{
    [GeneratedRegex(@".*\..*\.script-\d{3,6}\ .*\.sql$")]
    internal static partial Regex IsSqlScriptFile();
}
