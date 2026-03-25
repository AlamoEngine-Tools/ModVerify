using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

[JsonDerivedType(typeof(JsonVerificationError))]
[JsonDerivedType(typeof(JsonAggregatedVerificationError))]
internal abstract class JsonVerificationErrorBase
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter<VerificationSeverity>))]
    public VerificationSeverity Severity { get; }

    [JsonPropertyName("asset")]
    public string Asset { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("verifiers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? VerifierChain { get; }
    
    protected JsonVerificationErrorBase(string id,
        VerificationSeverity severity,
        string? asset,
        string message,
        IReadOnlyList<string>? verifierChain)
    {
        Id = id;
        VerifierChain = verifierChain;
        Message = message;
        Severity = severity;
        Asset = asset ?? string.Empty;
    }
    protected JsonVerificationErrorBase(VerificationError error, bool verbose = false)
    {
        Id = error.Id;
        VerifierChain = verbose ? error.VerifierChain.Select(x => x.Name).ToList() : null;
        Message = error.Message;
        Severity = error.Severity;
        Asset = error.Asset;
    }
}