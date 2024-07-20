using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Language;

internal sealed class FocGameLanguageManager(IServiceProvider serviceProvider) : GameLanguageManager(serviceProvider)
{
    public override IEnumerable<LanguageType> SupportedLanguages { get; } = new HashSet<LanguageType>
    {
        LanguageType.English, LanguageType.German, LanguageType.French,
        LanguageType.Spanish, LanguageType.Italian, LanguageType.Russian,
        LanguageType.Polish
    };

    public override LanguageType GetLanguagesFromUser()
    {
        var language = base.GetLanguagesFromUser();
        return language is LanguageType.Thai or LanguageType.Chinese or LanguageType.Japanese or LanguageType.Korean
            ? LanguageType.English
            : language;
    }
}