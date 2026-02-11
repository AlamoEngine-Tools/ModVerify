using Microsoft.Extensions.Logging;

namespace AET.ModVerify.App;

internal static class ModVerifyConstants
{
    public const string AppNameString = "AET Mod Verify";
    public const string ModVerifyToolId = "AET.ModVerify";
    public const string ModVerifyToolPath = "ModVerify";
    public const int ConsoleEventIdValue = 1138;

    public const int Success = 0;
    public const int CompletedWithFindings = 1;
    public const int ErrorBadArguments = 0xA0;

    public static readonly EventId ConsoleEventId = new(ConsoleEventIdValue, "LogToConsole");
}