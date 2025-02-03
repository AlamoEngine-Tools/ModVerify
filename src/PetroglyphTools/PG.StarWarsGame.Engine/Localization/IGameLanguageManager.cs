using System;
using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Localization;

public interface IGameLanguageManager
{ 
    IEnumerable<LanguageType> SupportedLanguages { get; }

    bool TryGetLanguage(string languageName, out LanguageType language);

    string LocalizeFileName(string fileName, LanguageType language, out bool localized);

    int LocalizeFileName(ReadOnlySpan<char> fileName, LanguageType language, Span<char> destination, out bool localized);

    bool IsFileNameLocalizable(ReadOnlySpan<char> fileName, bool requireEnglishName);

    LanguageType GetLanguagesFromUser();
}