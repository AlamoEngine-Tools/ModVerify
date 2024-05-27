using System.Collections.Generic;

namespace PG.StarWarsGame.Engine.Language;

public interface IGameLanguageManager
{
    IReadOnlyCollection<LanguageType> FocSupportedLanguages { get; }

    IReadOnlyCollection<LanguageType> EawSupportedLanguages { get; }

    string LocalizeFileName(string fileName, LanguageType language, out bool localized);
}