using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationError
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("verifiers")]
    public IReadOnlyList<string> VerifierChain { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter<VerificationSeverity>))]
    public VerificationSeverity Severity { get; }

    [JsonPropertyName("context")]
    public IEnumerable<string> ContextEntries { get; }

    [JsonPropertyName("asset")]
    public string Asset { get; }

    [JsonConstructor]
    private JsonVerificationError(
        string id, 
        IReadOnlyList<string>? verifierChain, 
        string message,
        VerificationSeverity severity, 
        IEnumerable<string>? contextEntries, 
        string asset)
    {
        Id = id;
        VerifierChain = verifierChain ?? [];
        Message = message;
        Severity = severity;
        ContextEntries = contextEntries ?? [];
        Asset = asset ?? string.Empty;
    }

    public JsonVerificationError(VerificationError error)
    {
        Id = error.Id;
        VerifierChain = error.VerifierChain;
        Message = error.Message;
        Severity = error.Severity;
        ContextEntries = error.ContextEntries;
        Asset = error.Asset;
    }
}