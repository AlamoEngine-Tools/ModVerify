using System.Diagnostics.CodeAnalysis;

namespace AET.ModVerifyTool.Updates;

internal readonly struct UpdateInfo
{
    public string DownloadLink { get; init; }

    public string? NewVersion { get; init; }

    [MemberNotNullWhen(true, nameof(DownloadLink), nameof(NewVersion))]
    public bool IsUpdateAvailable { get; init; }

}