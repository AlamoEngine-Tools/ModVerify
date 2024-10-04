using System;
using System.Collections.Generic;
using PG.Commons.Utilities;

namespace PG.StarWarsGame.Engine.Language;

public interface IGameLanguageManager
{ 
    IEnumerable<LanguageType> SupportedLanguages { get; }

    bool TryGetLanguage(string languageName, out LanguageType language);

    string LocalizeFileName(string fileName, LanguageType language, out bool localized);

    void LocalizeFileName(ReadOnlySpan<char> fileName, LanguageType language, ref ValueStringBuilder stringBuilder, out bool localized);

    bool IsFileNameLocalizable(ReadOnlySpan<char> fileName, bool requireEnglishName);

    LanguageType GetLanguagesFromUser();
}