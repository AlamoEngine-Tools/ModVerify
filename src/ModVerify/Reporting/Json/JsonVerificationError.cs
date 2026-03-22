using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationError : JsonVerificationErrorBase
{
    [JsonPropertyName("context")]
    [JsonPropertyOrder(99)]
    public IEnumerable<string> ContextEntries { get; }

    [JsonConstructor]
    public JsonVerificationError(
        string id,
        VerificationSeverity severity,
        string? asset,
        string message,
        IReadOnlyList<string>? verifierChain,
        IEnumerable<string> contextEntries) : base(id, severity, asset, message, verifierChain)
    {
        ContextEntries = contextEntries;
    }

    public JsonVerificationError(VerificationError error) : base(error)
    {
        ContextEntries = error.ContextEntries.Any() ? error.ContextEntries : [];
    }
}