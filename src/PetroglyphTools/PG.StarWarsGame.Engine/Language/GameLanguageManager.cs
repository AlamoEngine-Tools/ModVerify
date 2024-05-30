using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.Language;

internal sealed class GameLanguageManager(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IGameLanguageManager
{
    private static readonly IDictionary<LanguageType, string> LanguageToFileSuffixMapMp3 =
        new Dictionary<LanguageType, string>
        {
            { LanguageType.English, "_ENG.MP3" },
            { LanguageType.German, "_GER.MP3" },
            { LanguageType.French, "_FRE.MP3" },
            { LanguageType.Spanish, "_SPA.MP3" },
            { LanguageType.Italian, "_ITA.MP3" },
            { LanguageType.Japanese, "_JAP.MP3" },
            { LanguageType.Korean, "_KOR.MP3" },
            { LanguageType.Chinese, "_CHI.MP3" },
            { LanguageType.Russian, "_RUS.MP3" },
            { LanguageType.Polish, "_POL.MP3" },
            { LanguageType.Thai, "_THA.MP3" },
        };

    private static readonly IDictionary<LanguageType, string> LanguageToFileSuffixMapWav =
        new Dictionary<LanguageType, string>
        {
            { LanguageType.English, "_ENG.WAV" },
            { LanguageType.German, "_GER.WAV" },
            { LanguageType.French, "_FRE.WAV" },
            { LanguageType.Spanish, "_SPA.WAV" },
            { LanguageType.Italian, "_ITA.WAV" },
            { LanguageType.Japanese, "_JAP.WAV" },
            { LanguageType.Korean, "_KOR.WAV" },
            { LanguageType.Chinese, "_CHI.WAV" },
            { LanguageType.Russian, "_RUS.WAV" },
            { LanguageType.Polish, "_POL.WAV" },
            { LanguageType.Thai, "_THA.WAV" },
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


    public bool IsFileNameLocalizable(string fileName, bool requiredEnglishName)
    {
        var fileSpan = fileName.AsSpan();

        if (requiredEnglishName)
        {
            if (fileSpan.EndsWith("_ENG.WAV".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
            if (fileSpan.EndsWith("_ENG.MP3".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        var isWav = fileSpan.EndsWith(".WAV".AsSpan(), StringComparison.OrdinalIgnoreCase);
        var isMp3 = fileSpan.EndsWith(".MP3".AsSpan(), StringComparison.OrdinalIgnoreCase);

        ICollection<string>? checkList = null;
        if (isWav)
            checkList = LanguageToFileSuffixMapWav.Values;
        else if (isMp3)
            checkList = LanguageToFileSuffixMapMp3.Values;

        if (checkList is null)
            return false;

        foreach (var toCheck in checkList)
        {
            if (fileSpan.EndsWith(toCheck.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }


    public string LocalizeFileName(string fileName, LanguageType language, out bool localized)
    {
        if (fileName.Length > 260)
            throw new ArgumentOutOfRangeException(nameof(fileName), "fileName is too long");

        localized = true;

        // The game assumes that all localized audio files are referenced by using their english name.
        // Thus, PG takes this shortcut
        if (language == LanguageType.English)
            return fileName;

        var fileSpan = fileName.AsSpan();

        var isWav = false;
        var isMp3 = false;

        // The game only localizes file names iff they have the english suffix
        // NB: Also note that the engine does *not* check whether the filename actually ends with this suffix
        // but instead only take the first occurrence. This means that a file name like
        // 'test_eng.wav_ger.wav'
        // will trick the algorithm.
        var engSuffixIndex = fileSpan.IndexOf("_ENG.WAV".AsSpan(), StringComparison.OrdinalIgnoreCase);
        if (engSuffixIndex != -1)
            isWav = true;

        if (!isWav)
        {
            engSuffixIndex = fileSpan.IndexOf("_ENG.MP3".AsSpan(), StringComparison.OrdinalIgnoreCase);
            if (engSuffixIndex != -1) 
                isMp3 = true;
        }


        if (engSuffixIndex == -1)
        {
            localized = false;
            Logger.LogWarning($"Unable to localize '{fileName}'");
            return fileName;
        }

        var withoutSuffix = fileSpan.Slice(0, engSuffixIndex);
        
        ReadOnlySpan<char> newLocalizedSuffix;
        if (isWav)
            newLocalizedSuffix = LanguageToFileSuffixMapWav[language].AsSpan();
        else if (isMp3)
            newLocalizedSuffix = LanguageToFileSuffixMapMp3[language].AsSpan();
        else
            throw new InvalidOperationException();

        // 260 is roughly the MAX file path size on default Windows,
        // so we don't expect game files to be larger.
        var sb = new ValueStringBuilder(stackalloc char[260]);

        sb.Append(withoutSuffix);
        sb.Append(newLocalizedSuffix);

        return sb.ToString();
    }
}