using System.Collections.Generic;
using PG.StarWarsGame.Engine.GameManagers;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Engine.Repositories;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabase
{
    IGameRepository GameRepository { get; }

    IGameConstants GameConstants { get; }

    IGuiDialogManager GuiDialogManager { get; }

    IGameObjectTypeGameManager GameObjectTypeManager { get; } 

    ISfxEventGameManager SfxGameManager { get; }

    IEnumerable<LanguageType> InstalledLanguages { get; }
}