using System.Collections.Generic;

namespace PG.StarWarsGame.Engine;

public static class FocHardcodedConstants
{
    /// <summary>
    /// These models are hardcoded into StarWarsG.exe.
    /// </summary>
    public static IList<string> HardcodedModels { get; } = new List<string>
    {
        "i_tutorial_arrow.alo",
        "p_hero_empire_fx.alo",
        "i_tactical_corrupt.alo",
        "p_icon_corrupt.alo",
        "w_planet_select_neutral.alo",
        "i_game_arrow.alo",
        "i_galactic_radar.alo",
        "W_TextScroll.alo"
    };
}