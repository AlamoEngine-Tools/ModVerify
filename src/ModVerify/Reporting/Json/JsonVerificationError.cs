using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationError : JsonVerificationErrorBase
{
    [JsonPropertyName("context")]
    [JsonPropertyOrder(99)]
    public IEnumerable<string>? ContextEntries { get; }

    [JsonConstructor]
    public JsonVerificationError(
        string id, 
        IReadOnlyList<string>? verifierChain, 
        string message,
        VerificationSeverity severity, 
        IEnumerable<string>? contextEntries, 
        string? asset) : base(id, verifierChain, message, severity, asset)
    {
        ContextEntries = contextEntries;
    }

    public JsonVerificationError(VerificationError error) : base(error)
    {
        ContextEntries = error.ContextEntries.Any() ? error.ContextEntries : null;
    }
}