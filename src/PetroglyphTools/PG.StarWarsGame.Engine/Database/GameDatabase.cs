using System.Collections.Generic;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.Database;

internal class GameDatabase : IGameDatabase
{
    public required ICommandBarGameManager CommandBarManager { get; init; }

    public required IGameRepository GameRepository { get; init; }

    public required IGameConstants GameConstants { get; init; }

    public required IGuiDialogManager GuiDialogManager { get; init; }

    public required IGameObjectTypeGameManager GameObjectTypeManager { get; init; }

    public required ISfxEventGameManager SfxGameManager { get; init; }

    public required IEnumerable<LanguageType> InstalledLanguages { get; init; }
}