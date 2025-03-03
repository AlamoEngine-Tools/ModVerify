using System.Collections.Generic;

namespace PG.StarWarsGame.Files.ALO.Data;

public sealed class AlamoParticle : IAloDataContent
{
    public required string Name { get; init; }

    public ISet<string> Textures { get; init; }

    public void Dispose()
    {
    }
}