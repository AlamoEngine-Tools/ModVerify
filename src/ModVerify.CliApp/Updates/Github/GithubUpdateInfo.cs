using System.Diagnostics.CodeAnalysis;

namespace AET.ModVerify.App.Updates.Github;

internal readonly struct GithubUpdateInfo
{
    public string DownloadLink { get; init; }

    public string? NewVersion { get; init; }

    [MemberNotNullWhen(true, nameof(DownloadLink), nameof(NewVersion))]
    public bool IsUpdateAvailable { get; init; }

}