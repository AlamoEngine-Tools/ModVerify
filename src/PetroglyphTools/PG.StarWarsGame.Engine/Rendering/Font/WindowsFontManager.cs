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
            foreach (var windowsFontData in GetFonts(hdc))
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

    static IEnumerable<(Gdi32.ENUMLOGFONTEXDV lpelfe, Gdi32.ENUMTEXTMETRIC lpntme, Gdi32.FontType FontType)> GetFonts(Gdi32.SafeHDC hdc)
    {
        return Gdi32.EnumFontFamiliesEx(hdc, CharacterSet.DEFAULT_CHARSET);
    }
}