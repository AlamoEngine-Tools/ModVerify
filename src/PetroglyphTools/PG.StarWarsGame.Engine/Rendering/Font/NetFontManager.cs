using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Rendering.Font;

internal class NetFontManager : IOSFontManager
{
    public IEnumerable<string> GetFontFamilies()
    {
        // TODO: At the moment we do not have a platform-independent way to get font names.
        yield return "Arial";
        yield return "Arial Medium";
    }
}