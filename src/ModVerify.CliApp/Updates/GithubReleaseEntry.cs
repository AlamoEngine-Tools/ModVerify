using System.Text.Json.Serialization;

namespace AET.ModVerifyTool.Updates;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Skip)]
[method: JsonConstructor]
internal sealed class GithubReleaseEntry(string tag, string branch, bool isPrerelease)
{
    [JsonPropertyName("tag_name")]
    public string Tag { get; } = tag;

    [JsonPropertyName("target_commitish")]
    public string Branch { get; } = branch;

    [JsonPropertyName("prerelease")]
    public bool IsPrerelease { get; } = isPrerelease;
}