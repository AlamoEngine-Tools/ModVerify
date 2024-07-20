using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationError
{
    [JsonPropertyName("id")]
    public string Id { get; }

    [JsonPropertyName("verifier")]
    public string Verifier { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("severity")]
    public VerificationSeverity Severity { get; }

    [JsonPropertyName("assets")]
    public IEnumerable<string> Assets { get; }

    [JsonConstructor]
    private JsonVerificationError(string id, string verifier, string message, VerificationSeverity severity, IEnumerable<string> assets)
    {
        Id = id;
        Verifier = verifier;
        Message = message;
        Severity = severity;
        Assets = assets;
    }

    public JsonVerificationError(VerificationError error)
    {
        Id = error.Id;
        Verifier = error.Verifier;
        Message = error.Message;
        Severity = error.Severity;
        Assets = error.AffectedAssets;
    }
}