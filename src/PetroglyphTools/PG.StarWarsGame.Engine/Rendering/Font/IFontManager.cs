using System.Collections.Generic;
using PG.StarWarsGame.Engine.Localization;

namespace PG.StarWarsGame.Engine.Rendering.Font;

public interface IFontManager
{
    public IReadOnlyCollection<string> FontNames { get; }

    FontData? CreateFont(string fontName, int size, bool bold, bool italic, bool stat, float stretchFactor);

    public void NormalizeFontData(LanguageType language, ref string fontName, ref int fontSize);
}