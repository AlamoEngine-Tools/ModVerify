using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationReportMetadata
{
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public VerificationCompletionStatus Status { get; init; }

    [JsonPropertyName("target")]
    public JsonVerificationTarget Target { get; init; }

    [JsonPropertyName("time")]
    public string Date { get; } = DateTime.Now.ToString("s");

    [JsonPropertyName("duration")]
    public string Duration { get; init; }

    [JsonPropertyName("verifiers")]
    public IReadOnlyCollection<string> Verifiers { get; init; }
}