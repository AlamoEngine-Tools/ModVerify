using System.Text.Json.Serialization;
using AET.ModVerify.Reporting.Baseline;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationTarget
{
    [JsonPropertyName("name")]
    public string Name { get; }

    [JsonPropertyName("engine")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GameEngineType Engine { get; }

    [JsonPropertyName("isGame")]
    public bool IsGame { get; }

    [JsonPropertyName("version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonGameLocation? Location { get; }

    [JsonConstructor]
    private JsonVerificationTarget(
        string name,
        string? version,
        JsonGameLocation? location,
        GameEngineType engine,
        bool isGame)
    {
        Name = name;
        Version = version;
        Engine = engine;
        Location = location;
        IsGame = isGame;
    }

    public JsonVerificationTarget(VerificationTarget target)
    {
        Name = target.Name;
        Version = target.Version;
        Engine = target.Engine;
        Location = new JsonGameLocation(target.Location);
        IsGame = target.IsGame;
    }

    public JsonVerificationTarget(BaselineVerificationTarget target)
    {
        Name = target.Name;
        Version = target.Version;
        Engine = target.Engine;
        Location = target.Location is null ? null : new JsonGameLocation(target.Location);
        IsGame = target.IsGame;
    }

    public static BaselineVerificationTarget? ToTarget(JsonVerificationTarget? jsonTarget)
    {
        if (jsonTarget is null)
            return null;
        return new BaselineVerificationTarget
        {
            Engine = jsonTarget.Engine,
            Name = jsonTarget.Name,
            Location = JsonGameLocation.ToLocation(jsonTarget.Location),
            Version = jsonTarget.Version,
            IsGame = jsonTarget.IsGame
        };
    }
}