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

    [JsonPropertyName("verifiers")]
    public IReadOnlyList<string> VerifierChain { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter<VerificationSeverity>))]
    public VerificationSeverity Severity { get; }

    [JsonPropertyName("asset")]
    public string Asset { get; }

    protected JsonVerificationErrorBase(
        string id,
        IReadOnlyList<string>? verifierChain,
        string message,
        VerificationSeverity severity,
        string? asset)
    {
        Id = id;
        VerifierChain = verifierChain ?? [];
        Message = message;
        Severity = severity;
        Asset = asset ?? string.Empty;
    }

    protected JsonVerificationErrorBase(VerificationError error)
    {
        Id = error.Id;
        VerifierChain = error.VerifierChain.Select(x => x.Name).ToList();
        Message = error.Message;
        Severity = error.Severity;
        Asset = error.Asset;
    }
}

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