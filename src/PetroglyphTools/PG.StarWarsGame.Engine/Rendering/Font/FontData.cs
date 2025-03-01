namespace PG.StarWarsGame.Engine.Rendering.Font;

public readonly struct FontData
{
    public int FontSize { get; init; }

    public string FontName { get; init; }

    public bool Bold { get; init; }

    public bool Italic { get; init; }

    public bool StaticSize { get; init; }

    public float StretchFactor { get; init; }
}