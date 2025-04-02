using System.Collections.Generic;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;

namespace PG.StarWarsGame.Engine;

internal sealed class GameEngine : IStarWarsGameEngine
{
    public required GameEngineType EngineType { get; init; }

    public required IPGRender PGRender { get; init; }

    public required IFontManager FontManager { get; init; }

    public required ICommandBarGameManager CommandBar { get; init; }

    public required IGameRepository GameRepository { get; init; }

    public required IGameConstants GameConstants { get; init; }

    public required IGuiDialogManager GuiDialogManager { get; init; }

    public required IGameObjectTypeGameManager GameObjectTypeManager { get; init; }

    public required ISfxEventGameManager SfxGameManager { get; init; }

    public required IEnumerable<LanguageType> InstalledLanguages { get; init; }
}