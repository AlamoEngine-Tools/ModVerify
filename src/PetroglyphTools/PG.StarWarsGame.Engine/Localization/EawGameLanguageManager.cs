using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Localization;

internal sealed class EawGameLanguageManager(IServiceProvider serviceProvider) : GameLanguageManager(serviceProvider)
{
    public override IEnumerable<LanguageType> SupportedLanguages { get; } = new HashSet<LanguageType>
    {
        LanguageType.English, LanguageType.German, LanguageType.French,
        LanguageType.Spanish, LanguageType.Italian, LanguageType.Japanese,
        LanguageType.Korean, LanguageType.Chinese, LanguageType.Russian,
        LanguageType.Polish, LanguageType.Thai
    };
}