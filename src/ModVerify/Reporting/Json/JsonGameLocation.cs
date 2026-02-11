using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting.Json;

internal class JsonGameLocation
{
    [JsonPropertyName("modPaths")] 
    public IReadOnlyList<string> ModPaths { get; }
    
    [JsonPropertyName("gamePath")] 
    public string GamePath { get; }
    
    [JsonPropertyName("fallbackPaths")] 
    public IReadOnlyList<string> FallbackPaths { get; }

    [JsonConstructor]
    private JsonGameLocation(IReadOnlyList<string> modPaths, string gamePath, IReadOnlyList<string> fallbackPaths)
    {
        ModPaths = modPaths;
        GamePath = gamePath;
        FallbackPaths = fallbackPaths;
    }
    
    public JsonGameLocation(GameLocations location)
    {
        ModPaths = location.ModPaths.ToArray();
        GamePath = location.GamePath;
        FallbackPaths = location.FallbackPaths.ToArray();
    }

    public static GameLocations? ToLocation(JsonGameLocation? jsonLocation)
    {
        return jsonLocation is null
            ? null
            : new GameLocations(jsonLocation.ModPaths, jsonLocation.GamePath, jsonLocation.FallbackPaths);
    }
}