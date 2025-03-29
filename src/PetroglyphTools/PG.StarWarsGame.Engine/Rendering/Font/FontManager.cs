using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.Rendering.Font;

internal class FontManager : GameManagerBase, IFontManager
{
    private readonly IOSFontManager _fontManager;

    private ISet<string> _fontNames = null!;

    public FontManager(GameRepository repository, GameErrorReporterWrapper errorReporter, IServiceProvider serviceProvider) 
        : base(repository, errorReporter, serviceProvider)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _fontManager = new WindowsFontManager();
        else
            _fontManager = new NetFontManager();
    }

    public IReadOnlyCollection<string> FontNames => [.._fontNames];

    public FontData? CreateFont(string fontName, int size, bool bold, bool italic, bool staticSize, float stretchFactor)
    {
        ThrowIfNotInitialized();

        if (size <= 0)
        {
            ErrorReporter.Assert(EngineAssert.Create(EngineAssertKind.ValueOutOfRange, size, [fontName], 
                $"Font size '{size}' for '{fontName}' must be greater than 0."));
        }

        if (!_fontNames.Contains(fontName)) 
            fontName = PGConstants.DefaultUnicodeFontName;

        if (!_fontNames.Contains(fontName))
        {
            ErrorReporter.Assert(EngineAssert.FromNullOrEmpty([fontName], $"Unable to find font '{fontName}'"));
            return null;
        }

        return new FontData();
    }

    public void NormalizeFontData(LanguageType language, ref string fontName, ref int fontSize)
    {
        if (language >= LanguageType.Japanese)
            fontName = PGConstants.DefaultUnicodeFontName;
        if (language == LanguageType.Russian && fontSize < 7)
            fontSize = 7;
    }

    protected override Task InitializeCoreAsync(CancellationToken token)
    {
        var fontNames = new HashSet<string>(_fontManager.GetFontFamilies(), StringComparer.OrdinalIgnoreCase)
        {
            "EmpireAtWar-Bold",
            "EmpireAtWar-Light",
            "EmpireAtWar-Medium",
            "EmpireAtWar-Stencil"
        };
        _fontNames = fontNames;
        return Task.CompletedTask;
    }
}