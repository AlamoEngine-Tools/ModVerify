using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Language;

public interface IGameLanguageManager
{
    IReadOnlyCollection<LanguageType> FocSupportedLanguages { get; }

    IReadOnlyCollection<LanguageType> EawSupportedLanguages { get; }

    bool TryGetLanguage(string languageName, out LanguageType language);

    string LocalizeFileName(string fileName, LanguageType language, out bool localized);

    bool IsFileNameLocalizable(string fileName, bool requireEnglishName);
}