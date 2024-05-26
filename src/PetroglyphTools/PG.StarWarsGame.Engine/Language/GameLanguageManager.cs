using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Language;

public static class GameLanguageManager
{
    public static readonly ISet<LanguageType> FocSupportedLanguages = new HashSet<LanguageType>
    {
        LanguageType.English, LanguageType.German, LanguageType.French, 
        LanguageType.Spanish, LanguageType.Italian, LanguageType.Russian,
        LanguageType.Polish
    };

    public static readonly ISet<LanguageType> EawSupportedLanguages = new HashSet<LanguageType>
    {
        LanguageType.English, LanguageType.German, LanguageType.French,
        LanguageType.Spanish, LanguageType.Italian, LanguageType.Japanese, 
        LanguageType.Korean, LanguageType.Chinese, LanguageType.Russian,
        LanguageType.Polish, LanguageType.Thai
    };
}