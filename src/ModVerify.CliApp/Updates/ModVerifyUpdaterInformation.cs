using System.Diagnostics;
using Semver;

namespace AET.ModVerifyTool.Updates;

internal static class ModVerifyUpdaterInformation
{
    public const string BranchName = "main";
    public const string GithubReleasesApiLink = "https://api.github.com/repos/AlamoEngine-Tools/ModVerify/releases";
    public const string ModVerifyReleasesDownloadLink = "https://github.com/AlamoEngine-Tools/ModVerify/releases/latest";
    public const string UserAgent = "AET.Modifo";

    public static readonly SemVersion? CurrentVersion;

    static ModVerifyUpdaterInformation()
    {
        var currentAssembly = typeof(ModVerifyUpdaterInformation).Assembly;
        var fi = FileVersionInfo.GetVersionInfo(currentAssembly.Location);
        SemVersion.TryParse(fi.ProductVersion, SemVersionStyles.Any, out var currentVersion);
        CurrentVersion = currentVersion;
    }
}