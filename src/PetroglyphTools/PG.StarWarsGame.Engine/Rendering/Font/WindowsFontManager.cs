using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Vanara.PInvoke;

namespace PG.StarWarsGame.Engine.Rendering.Font;

[SupportedOSPlatform("windows")]
internal class WindowsFontManager : IOSFontManager
{
    public IEnumerable<string> GetFontFamilies()
    {
        var hdc = Gdi32.CreateCompatibleDC();

        var fonts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var windowsFontData in Gdi32.EnumFontFamiliesEx(hdc, CharacterSet.DEFAULT_CHARSET))
            {
                var fontName = windowsFontData.lpelfe.elfEnumLogfontEx.elfFullName;
                fonts.Add(fontName);
            }
        }
        finally
        {
            Gdi32.DeleteDC(hdc);
        }
        return fonts;
    }
}