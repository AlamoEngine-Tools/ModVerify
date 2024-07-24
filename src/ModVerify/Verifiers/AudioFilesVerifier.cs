using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using Microsoft.Extensions.DependencyInjection;
using PG.Commons.Hashing;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using PG.StarWarsGame.Engine.DataTypes;
using PG.StarWarsGame.Engine.Language;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace AET.ModVerify.Verifiers;

public class AudioFilesVerifier : GameVerifierBase
{
    private readonly PetroglyphDataEntryPathNormalizer _pathNormalizer;
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
        foreach (var sfxEvent in Database.SfxEvents.Entries)
        {
            foreach (var codedSample in sfxEvent.AllSamples)
            {
                VerifySample(codedSample, sfxEvent, languagesToVerify, visitedSamples);
            }
        }
    }

    private void VerifySample(string sample, SfxEvent sfxEvent, IEnumerable<LanguageType> languagesToVerify, HashSet<Crc32> visitedSamples)
    {
        Span<char> sampleNameBuffer = stackalloc char[PGConstants.MaxPathLength];

        var i = _pathNormalizer.Normalize(sample.AsSpan(), sampleNameBuffer);
        var normalizedSampleName = sampleNameBuffer.Slice(0, i);
        var crc = _hashingService.GetCrc32(normalizedSampleName, PGConstants.PGCrc32Encoding);
        if (!visitedSamples.Add(crc))
            return;

        
        if (normalizedSampleName.Length > PGConstants.MaxPathLength)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.FilePathTooLong,
                $"Sample name '{sample}' is too long.",
                VerificationSeverity.Error,
                sample));
            return;
        }

        var normalizedSampleNameString = normalizedSampleName.ToString();

        if (sfxEvent.IsLocalized)
        {
            foreach (var language in languagesToVerify)
            {
                var localizedSampleName = _languageManager.LocalizeFileName(normalizedSampleNameString, language, out var localized);
                VerifySample(localizedSampleName, sfxEvent);
                
                if (!localized)
                    return;
            }
        }
        else
        {
            VerifySample(normalizedSampleNameString, sfxEvent);
        }
    }

    private void VerifySample(string sample, SfxEvent sfxEvent)
    {
        using var sampleStream = Repository.TryOpenFile(sample);
        if (sampleStream is null)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotFound, 
                $"Audio file '{sample}' could not be found.", 
                VerificationSeverity.Error,
                sample));
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
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotPCM,
                $"Audio file '{sample}' has an invalid format '{format}'. Supported is {WaveFormats.PCM}", 
                VerificationSeverity.Error,
                sample));
        }

        if (channels > 1 && !IsAmbient2D(sfxEvent))
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotMono, 
                $"Audio file '{sample}' is not mono audio.", 
                VerificationSeverity.Information, 
                sample));
        }

        if (sampleRate > 48_000)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes. InvalidSampleRate, 
                $"Audio file '{sample}' has a too high sample rate of {sampleRate}. Maximum is 48.000Hz.",
                VerificationSeverity.Error,
                sample));
        }

        if (bitPerSecondPerChannel > 16)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.InvalidBitsPerSeconds, 
                $"Audio file '{sample}' has an invalid bit size of {bitPerSecondPerChannel}. Supported are 16bit.",
                VerificationSeverity.Error, 
                sample));
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