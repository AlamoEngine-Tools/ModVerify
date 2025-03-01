using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Rendering.Font;

internal interface IOSFontManager
{
    IEnumerable<string> GetFontFamilies();
}