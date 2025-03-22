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
    public VerificationSeverity Severity { get; }

    [JsonPropertyName("assets")]
    public IEnumerable<string> Assets { get; }

    [JsonConstructor]
    private JsonVerificationError(string id, IReadOnlyList<string> verifierChain, string message, VerificationSeverity severity, IEnumerable<string> assets)
    {
        Id = id;
        VerifierChain = verifierChain;
        Message = message;
        Severity = severity;
        Assets = assets;
    }

    public JsonVerificationError(VerificationError error)
    {
        Id = error.Id;
        VerifierChain = error.VerifierChain;
        Message = error.Message;
        Severity = error.Severity;
        Assets = error.AffectedAssets;
    }
}