using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonSuppressionFilter(SuppressionFilter filter)
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; } = filter.Id;

    [JsonPropertyName("Verifier")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Verifier { get; } = filter.Verifier;

    [JsonPropertyName("asset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Asset { get;  } = filter.Asset;
}