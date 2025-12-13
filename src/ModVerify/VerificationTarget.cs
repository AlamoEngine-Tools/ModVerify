using System;
using System.Text;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

public sealed class VerificationTarget
{
    public required GameEngineType Engine { get; init; }

    public required string Name
    {
        get;
        init
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            field = value;
        }
    }

    public required GameLocations Location
    {
        get;
        init => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string? Version { get; init; }

    public bool IsGame => Location.ModPaths.Count == 0;

    public override string ToString()
    {
        var sb = new StringBuilder($"[Name={Name};EngineType={Engine};");
        if (!string.IsNullOrEmpty(Version))
            sb.Append($"Version={Version};");
        sb.Append($"Location={Location};");
        sb.Append("]");
        return sb.ToString();
    }
}