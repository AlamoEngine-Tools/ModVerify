using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.Language;

internal sealed class GameLanguageManager(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IGameLanguageManager
{
    private static readonly IDictionary<LanguageType, string> LanguageToFileSuffixMap =
        new Dictionary<LanguageType, string>
        {
            { LanguageType.English, "_ENG"},
            { LanguageType.German, "_GER"},
            { LanguageType.French, "_FRE"},
            { LanguageType.Spanish, "_SPA"},
            { LanguageType.Italian, "_ITA"},
            { LanguageType.Japanese, "_JAP"},
            { LanguageType.Korean, "_KOR"},
            { LanguageType.Chinese, "_CHI"},
            { LanguageType.Russian, "_RUS"},
            { LanguageType.Polish, "_POL"},
            { LanguageType.Thai, "_THA"},
        };
    
    public IReadOnlyCollection<LanguageType> FocSupportedLanguages { get; } =
        new HashSet<LanguageType>
        {
            LanguageType.English, LanguageType.German, LanguageType.French,
            LanguageType.Spanish, LanguageType.Italian, LanguageType.Russian,
            LanguageType.Polish
        };

    public IReadOnlyCollection<LanguageType> EawSupportedLanguages { get; } =
        new HashSet<LanguageType>
        {
            LanguageType.English, LanguageType.German, LanguageType.French,
            LanguageType.Spanish, LanguageType.Italian, LanguageType.Japanese,
            LanguageType.Korean, LanguageType.Chinese, LanguageType.Russian,
            LanguageType.Polish, LanguageType.Thai
        };

    public string LocalizeFileName(string fileName, LanguageType language, out bool localized)
    {
        localized = true;

        if (language == LanguageType.English)
            return fileName;

        var upperFileName = fileName.ToUpper();

        var fileSpan = upperFileName.AsSpan();

#if NETSTANDARD2_1_OR_GREATER || NET
        var extension = FileSystem.Path.GetExtension(fileSpan);
#else
        var extension = FileSystem.Path.GetExtension(upperFileName).AsSpan();
#endif

        var withoutExtension = fileSpan.Slice(0, fileSpan.Length - extension.Length);
        var engSuffixIndex = withoutExtension.LastIndexOf("_ENG".AsSpan());

        if (engSuffixIndex == -1 ||
            (!extension.Equals(".MP3".AsSpan(), StringComparison.Ordinal) &&
             !extension.Equals(".WAV".AsSpan(), StringComparison.Ordinal)))
        {
            localized = false;
            Logger.LogTrace($"Unable to localize '{fileName}'");
            return fileName;
        }

        var withoutSuffix = withoutExtension.Slice(0, engSuffixIndex);
        var localizedSuffix = LanguageToFileSuffixMap[language].AsSpan();

        return StringUtilities.Concat(withoutSuffix, localizedSuffix, extension);
    }
}