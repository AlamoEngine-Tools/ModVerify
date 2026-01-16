using System.Text.Json.Serialization;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationTarget
{
    [JsonPropertyName("name")] 
    public string Name { get; }

    [JsonPropertyName("engine")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GameEngineType Engine { get; }

    [JsonPropertyName("version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version{ get; }

    [JsonPropertyName("location")]
    public JsonGameLocation Location { get; }

    [JsonConstructor]
    private JsonVerificationTarget(string name, string? version, JsonGameLocation location, GameEngineType engine)
    {
        Name = name;
        Version = version;
        Engine = engine;
        Location = location;
    }

    public JsonVerificationTarget(VerificationTarget target)
    {
        Name = target.Name;
        Version = target.Version;
        Engine = target.Engine;
        Location = new JsonGameLocation(target.Location);
    }

    public static VerificationTarget? ToTarget(JsonVerificationTarget? jsonTarget)
    {
        if (jsonTarget is null)
            return null;
        return new VerificationTarget
        {
            Engine = jsonTarget.Engine,
            Name = jsonTarget.Name,
            Location = JsonGameLocation.ToLocation(jsonTarget.Location),
            Version = jsonTarget.Version
        };
    }
}