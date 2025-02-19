using PG.StarWarsGame.Engine.Rendering;

namespace PG.StarWarsGame.Engine.CommandBar.Components;

public sealed class CommandBarComponentTextData
{
    public CommandBarBaseComponent Parent { get; }

    public PrimRenderMode Mode { get; init; }

    public float Scale { get; init; }

    public int TempAlpha { get; init; }

    public RgbaColor Color { get; init; }

    public bool AutoPixelAlign { get; init; } = true;

    internal CommandBarComponentTextData(CommandBarBaseComponent parent)
    {
        Parent = parent;
    }
}