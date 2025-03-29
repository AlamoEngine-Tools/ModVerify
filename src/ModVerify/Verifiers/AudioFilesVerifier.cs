using System;
using System.Buffers;
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
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.Localization;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;

#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace AET.ModVerify.Verifiers;

public class AudioFilesVerifier : GameVerifier
{
    private static readonly PathNormalizeOptions SampleNormalizerOptions = new()
    {
        UnifyCase = UnifyCasingKind.UpperCaseForce,
        UnifySeparatorKind = DirectorySeparatorKind.Windows,
        UnifyDirectorySeparators = true
    };

    private readonly EmpireAtWarMegDataEntryPathNormalizer _pathNormalizer = EmpireAtWarMegDataEntryPathNormalizer.Instance;
    private readonly ICrc32HashingService _hashingService;
    private readonly IFileSystem _fileSystem;
    private readonly IGameLanguageManager _languageManager;

    public AudioFilesVerifier(IGameDatabase gameDatabase, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base(null, gameDatabase, settings, serviceProvider)
    {
        _hashingService = serviceProvider.GetRequiredService<ICrc32HashingService>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _languageManager = serviceProvider.GetRequiredService<IGameLanguageManagerProvider>()
            .GetLanguageManager(Repository.EngineType);
    }

    public override string FriendlyName => "Verify Audio Files";

    public override void Verify(CancellationToken token)
    {
        var visitedSamples = new HashSet<Crc32>();
        var languagesToVerify = GetLanguagesToVerify().ToList();
        foreach (var sfxEvent in Database.SfxGameManager.Entries)
        {
            foreach (var codedSample in sfxEvent.AllSamples)
            {
                VerifySample(codedSample.AsSpan(), sfxEvent, languagesToVerify, visitedSamples);
            }
        }
    }

    private void VerifySample(ReadOnlySpan<char> sample, SfxEvent sfxEvent, IEnumerable<LanguageType> languagesToVerify, HashSet<Crc32> visitedSamples)
    {
        char[]? pooledBuffer = null;

        var buffer = sample.Length < PGConstants.MaxMegEntryPathLength
            ? stackalloc char[PGConstants.MaxMegEntryPathLength]
            : pooledBuffer = ArrayPool<char>.Shared.Rent(sample.Length);

        try
        {
            var length = PathNormalizer.Normalize(sample, buffer, SampleNormalizerOptions);
            var sampleNameBuffer = buffer.Slice(0, length);

            var crc = _hashingService.GetCrc32(sampleNameBuffer, Encoding.ASCII);
            if (!visitedSamples.Add(crc))
                return;

            if (sfxEvent.IsLocalized)
            {
                foreach (var language in languagesToVerify)
                {
                    VerifySampleLocalized(sfxEvent, sampleNameBuffer, language, out var localized);
                    if (!localized)
                        return;
                }
            }
            else
            {
                VerifySample(sampleNameBuffer, sfxEvent);
            }
        }
        finally
        {
            if (pooledBuffer is not null)
                ArrayPool<char>.Shared.Return(pooledBuffer);
        }

    }

    private void VerifySampleLocalized(SfxEvent sfxEvent, ReadOnlySpan<char> sample, LanguageType language, out bool localized)
    {
        char[]? pooledBuffer = null;

        var buffer = sample.Length < PGConstants.MaxMegEntryPathLength
            ? stackalloc char[PGConstants.MaxMegEntryPathLength]
            : pooledBuffer = ArrayPool<char>.Shared.Rent(sample.Length);
        try
        {
            var l = _languageManager.LocalizeFileName(sample, language, buffer, out localized);
            var localizedName = buffer.Slice(0, l);
            VerifySample(localizedName, sfxEvent);
        }
        finally
        {
            if (pooledBuffer is not null)
                ArrayPool<char>.Shared.Return(pooledBuffer);
        }
    }

    private void VerifySample(ReadOnlySpan<char> sample, SfxEvent sfxEvent)
    {
        using var sampleStream = Repository.TryOpenFile(sample);
        if (sampleStream is null)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                VerifierChain,
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
                VerifierChain,
                VerifierErrorCodes.SampleNotPCM,
                $"Audio file '{sampleString}' has an invalid format '{format}'. Supported is {WaveFormats.PCM}", 
                VerificationSeverity.Error,
                sampleString));
        }

        if (channels > 1 && !IsAmbient2D(sfxEvent))
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes.SampleNotMono, 
                $"Audio file '{sampleString}' is not mono audio.", 
                VerificationSeverity.Information,
                sampleString));
        }

        if (sampleRate > 48_000)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                VerifierChain,
                VerifierErrorCodes. InvalidSampleRate, 
                $"Audio file '{sampleString}' has a too high sample rate of {sampleRate}. Maximum is 48.000Hz.",
                VerificationSeverity.Error,
                sampleString));
        }

        if (bitPerSecondPerChannel > 16)
        {
            var sampleString = sample.ToString();
            AddError(VerificationError.Create(
                VerifierChain,
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