using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.GameConstants;
using PG.StarWarsGame.Engine.GameObjects;
using PG.StarWarsGame.Engine.GuiDialog;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Engine.Rendering;
using PG.StarWarsGame.Engine.Rendering.Font;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Database;

public interface IGameDatabase
{
    IPGRender PGRender { get; }

    IFontManager FontManager { get; }

    IGameRepository GameRepository { get; }

    IGameConstants GameConstants { get; }

    IGuiDialogManager GuiDialogManager { get; }

    IGameObjectTypeGameManager GameObjectTypeManager { get; } 

    ISfxEventGameManager SfxGameManager { get; }

    IEnumerable<LanguageType> InstalledLanguages { get; }
}