﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PG.Commons.Services;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.Localization;

internal abstract class GameLanguageManager(IServiceProvider serviceProvider) : ServiceBase(serviceProvider), IGameLanguageManager
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

    public abstract IEnumerable<LanguageType> SupportedLanguages { get; }

    public bool TryGetLanguage(string languageName, out LanguageType language)
    {
        language = LanguageType.English;
        return Enum.TryParse(languageName, true, out language);
    }
    
    public bool IsFileNameLocalizable(ReadOnlySpan<char> fileName, bool requiredEnglishName)
    {
        if (requiredEnglishName)
        {
            if (fileName.EndsWith("_ENG.WAV".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
            if (fileName.EndsWith("_ENG.MP3".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        var isWav = fileName.EndsWith(".WAV".AsSpan(), StringComparison.OrdinalIgnoreCase);
        var isMp3 = fileName.EndsWith(".MP3".AsSpan(), StringComparison.OrdinalIgnoreCase);

        ICollection<string>? checkList = null;
        if (isWav)
            checkList = LanguageToFileSuffixMapWav.Values;
        else if (isMp3)
            checkList = LanguageToFileSuffixMapMp3.Values;

        if (checkList is null)
            return false;

        foreach (var toCheck in checkList)
        {
            if (fileName.EndsWith(toCheck.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public virtual LanguageType GetLanguagesFromUser()
    {
        CultureInfo culture;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var langId = GetUserDefaultUILanguage();
            culture = new CultureInfo(langId);
        }
        else
            culture = CultureInfo.CurrentCulture;

        var rootCulture = GetParentCultureRecursive(culture);

        var cultureName = rootCulture.EnglishName;

        if (TryGetLanguage(cultureName, out var language))
            return language;
        return LanguageType.English;
    }

    private CultureInfo GetParentCultureRecursive(CultureInfo culture)
    {
        var parent = culture.Parent;
        if (parent.Equals(CultureInfo.InvariantCulture))
            return culture;
        return GetParentCultureRecursive(parent);
    }


    public string LocalizeFileName(string fileName, LanguageType language, out bool localized)
    {
        // The game assumes that all localized audio files are referenced by using their english name.
        // Thus, PG takes this shortcut
        if (language == LanguageType.English)
        {
            localized = true;
            return fileName;
        }

        var stringBuilder = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        LocalizeFileName(fileName.AsSpan(), language, ref stringBuilder, out localized);
        if (!localized)
        {
            stringBuilder.Dispose();
            return fileName;
        }

        Debug.Assert(stringBuilder.Length == fileName.Length);
        return stringBuilder.ToString();
    }

    public int LocalizeFileName(ReadOnlySpan<char> fileName, LanguageType language, Span<char> destination, out bool localized)
    {
        var sb = new ValueStringBuilder(destination.Length);
        LocalizeFileName(fileName, language, ref sb, out localized);
        sb.TryCopyTo(destination, out var written);
        sb.Dispose();
        return written;
    }

    public void LocalizeFileName(ReadOnlySpan<char> fileName, LanguageType language, ref ValueStringBuilder stringBuilder, out bool localized)
    {
        localized = true;

        // The game assumes that all localized audio files are referenced by using their english name.
        // Thus, PG takes this shortcut
        if (language == LanguageType.English)
        {
            stringBuilder.Append(fileName);
            return;
        }

        var isWav = false;
        var isMp3 = false;

        // The game only localizes file names iff they have the english suffix
        // NB: Also note that the engine does *not* check whether the filename actually ends with this suffix
        // but instead only take the first occurrence. This means that a file name like 'test_eng.wav_ger.wav' will trick the algorithm.
        var engSuffixIndex = fileName.IndexOf("_ENG.WAV".AsSpan(), StringComparison.OrdinalIgnoreCase);
        if (engSuffixIndex != -1)
            isWav = true;

        if (!isWav)
        {
            engSuffixIndex = fileName.IndexOf("_ENG.MP3".AsSpan(), StringComparison.OrdinalIgnoreCase);
            if (engSuffixIndex != -1)
                isMp3 = true;
        }


        if (engSuffixIndex == -1)
        {
            localized = false;
            Logger.LogWarning($"Unable to localize '{fileName.ToString()}'");
            stringBuilder.Append(fileName);
            return;
        }

        var withoutSuffix = fileName.Slice(0, engSuffixIndex);

        ReadOnlySpan<char> newLocalizedSuffix;
        if (isWav)
            newLocalizedSuffix = LanguageToFileSuffixMapWav[language].AsSpan();
        else if (isMp3)
            newLocalizedSuffix = LanguageToFileSuffixMapMp3[language].AsSpan();
        else
            throw new InvalidOperationException();

        stringBuilder.Append(withoutSuffix);
        stringBuilder.Append(newLocalizedSuffix);
    }


    [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
    static extern ushort GetUserDefaultUILanguage();
}