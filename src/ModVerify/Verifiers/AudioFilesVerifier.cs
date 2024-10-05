using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.Commons.Utilities;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Audio;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace AET.ModVerify.Verifiers;

public class AudioFilesVerifier : GameVerifierBase
{
    private static readonly PathNormalizeOptions SampleNormalizerOptions = new()
    {
        UnifyCase = UnifyCasingKind.UpperCaseForce,
        UnifySeparatorKind = DirectorySeparatorKind.Windows,
        UnifyDirectorySeparators = true
    };

    private readonly EmpireAtWarMegDataEntryPathNormalizer _pathNormalizer;
    private readonly ICrc32HashingService _hashingService;
    private readonly IFileSystem _fileSystem;
    private readonly IGameLanguageManager _languageManager;

    public AudioFilesVerifier(IGameDatabase gameDatabase, GameVerifySettings settings, IServiceProvider serviceProvider) : base(gameDatabase, settings, serviceProvider)
    {
        _pathNormalizer = new(serviceProvider);
        _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _languageManager = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>()
            .GetLanguageManager(Repository.EngineType);
    }

    public override string FriendlyName => "Verify Audio Files";

    protected override void RunVerification(CancellationToken token)
    {
        var visitedSamples = new HashSet<Crc32>();
        var languagesToVerify = GetLanguagesToVerify().ToList();
        foreach (var sfxEvent in Database.SfxGameManager.Entries)
        {
            foreach (var codedSample in sfxEvent.AllSamples)
            {
                VerifySample(codedSample, sfxEvent, languagesToVerify, visitedSamples);
            }
        }
    }

    private void VerifySample(string sample, SfxEvent sfxEvent, IEnumerable<LanguageType> languagesToVerify, HashSet<Crc32> visitedSamples)
    {
        var sb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        var sampleNameBuffer = sb.AppendSpan(sample.Length);

        var i = PathNormalizer.Normalize(sample.AsSpan(), sampleNameBuffer, SampleNormalizerOptions);
        sb.Length = i;

        var crc = _hashingService.GetCrc32(sb.AsSpan(), Encoding.ASCII);
        if (!visitedSamples.Add(crc))
            return;
        
        if (sfxEvent.IsLocalized)
        {
            foreach (var language in languagesToVerify)
            {
                VerifySampleLocalized(sfxEvent, sb.AsSpan(), language, out var localized);
                if (!localized)
                    return;
            }
        }
        else
        { 
            VerifySample(sb.AsSpan(), sfxEvent);
        }
        sb.Dispose();
    }

    private void VerifySampleLocalized(SfxEvent sfxEvent, ReadOnlySpan<char> sample, LanguageType language, out bool localized)
    {
        var localizedSb = new ValueStringBuilder(stackalloc char[PGConstants.MaxMegEntryPathLength]);
        _languageManager.LocalizeFileName(sample, language, ref localizedSb, out localized);
        VerifySample(localizedSb.AsSpan(), sfxEvent);
        localizedSb.Dispose();
    }

    private void VerifySample(ReadOnlySpan<char> sample, SfxEvent sfxEvent)
    {
        using var sampleStream = Repository.TryOpenFile(sample);
        if (sampleStream is null)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotFound,
                $"Audio file '{sampleString}' could not be found.",
                VerificationSeverity.Error,
                sampleString));
            return;
        }
        using var binaryReader = new BinaryReader(sampleStream);

        // Skip Header + "fmt "
        binaryReader.BaseStream.Seek(16, SeekOrigin.Begin);

        var fmtSize = binaryReader.ReadInt32();
        var format = (WaveFormats)binaryReader.ReadInt16();
        var channels = binaryReader.ReadInt16();

        var sampleRate = binaryReader.ReadInt32();
        var bytesPerSecond = binaryReader.ReadInt32();

        var frameSize = binaryReader.ReadInt16();
        var bitPerSecondPerChannel = binaryReader.ReadInt16();

        if (format != WaveFormats.PCM)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotPCM,
                $"Audio file '{sampleString}' has an invalid format '{format}'. Supported is {WaveFormats.PCM}", 
                VerificationSeverity.Error,
                sampleString));
        }

        if (channels > 1 && !IsAmbient2D(sfxEvent))
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotMono, 
                $"Audio file '{sampleString}' is not mono audio.", 
                VerificationSeverity.Information,
                sampleString));
        }

        if (sampleRate > 48_000)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes. InvalidSampleRate, 
                $"Audio file '{sampleString}' has a too high sample rate of {sampleRate}. Maximum is 48.000Hz.",
                VerificationSeverity.Error,
                sampleString));
        }

        if (bitPerSecondPerChannel > 16)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.InvalidBitsPerSeconds, 
                $"Audio file '{sampleString}' has an invalid bit size of {bitPerSecondPerChannel}. Supported are 16bit.",
                VerificationSeverity.Error,
                sampleString));
        }
    }

    // Some heuristics whether a SFXEvent is most likely to be an ambient sound.
    private bool IsAmbient2D(SfxEvent sfxEvent)
    {
        if (!sfxEvent.Is2D)
            return false;

        if (sfxEvent.IsPreset)
            return false;

        // If the event is located in SFXEventsAmbient.xml we simply assume it's an ambient sound.
        var fileName = _fileSystem.Path.GetFileName(sfxEvent.Location.XmlFile.AsSpan());
        if (fileName.Equals("SFXEventsAmbient.xml".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrEmpty(sfxEvent.UsePresetName))
            return false;

        if (sfxEvent.UsePresetName!.StartsWith("Preset_AMB_2D"))
            return true;

        return true;
    }

    private IEnumerable<LanguageType> GetLanguagesToVerify()
    {
        switch (Settings.LocalizationOption)
        {
            case VerifyLocalizationOption.English:
                return new List<LanguageType> { LanguageType.English };
            case VerifyLocalizationOption.CurrentSystem:
                return new List<LanguageType> { _languageManager.GetLanguagesFromUser() };
            case VerifyLocalizationOption.AllInstalled:
                return Database.InstalledLanguages;
            case VerifyLocalizationOption.All:
                return _languageManager.SupportedLanguages;
            default:
                throw new NotSupportedException($"{Settings.LocalizationOption} is not supported");
        }
    }

    private enum WaveFormats
    {
        PCM = 1,
        MSADPCM = 2,
        IEEE_Float = 3,
    }
}