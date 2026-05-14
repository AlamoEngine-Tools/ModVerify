using System;
using System.Collections.Generic;
using System.Threading;
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

internal sealed class GameEngine : IStarWarsGameEngineHandle
{
    private int _disposed;

    private PetroglyphFileSystem? _pgFileSystem;

    public required GameEngineType EngineType { get; init; }

    public required IPGRender PGRender
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required IFontManager FontManager
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required ICommandBarGameManager CommandBar
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required IGameRepository GameRepository
    {
        get { ThrowIfDisposed(); return field; }
        init
        {
            field = value;
            _pgFileSystem = value.PGFileSystem;
        }
    }

    public required IGameConstants GameConstants
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required IGuiDialogManager GuiDialogManager
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required IGameObjectTypeGameManager GameObjectTypeManager
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required ISfxEventGameManager SfxGameManager
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public required IEnumerable<LanguageType> InstalledLanguages
    {
        get { ThrowIfDisposed(); return field; }
        init;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        _pgFileSystem?.CleanupStrategy();
        _pgFileSystem = null;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed != 0)
            throw new ObjectDisposedException(nameof(GameEngine));
    }
}
