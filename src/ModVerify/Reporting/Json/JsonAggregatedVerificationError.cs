using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonAggregatedVerificationError : JsonVerificationErrorBase
{
    [JsonPropertyName("contexts")]
    [JsonPropertyOrder(99)]
    public IEnumerable<IEnumerable<string>> Contexts { get; }

    [JsonConstructor]
    public JsonAggregatedVerificationError(
        string id,
        IReadOnlyList<string>? verifierChain,
        string message,
        VerificationSeverity severity,
        IEnumerable<IEnumerable<string>>? contexts,
        string? asset) : base(id, verifierChain, message, severity, asset)
    {
        Contexts = contexts ?? [];
    }

    public JsonAggregatedVerificationError(
        VerificationError error,
        IEnumerable<IEnumerable<string>> contexts) : base(error)
    {
        Contexts = contexts;
    }
}