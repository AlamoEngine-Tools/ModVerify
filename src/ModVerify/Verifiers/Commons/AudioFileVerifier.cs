using System;
using System.Collections.Generic;
using System.IO;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using AET.ModVerify.Verifiers.Caching;

namespace AET.ModVerify.Verifiers.Commons;

public class AudioFileVerifier : GameVerifier<AudioFileInfo>
{
    private readonly IAlreadyVerifiedCache? _alreadyVerifiedCache;

    public AudioFileVerifier(GameVerifierBase parent) : base(parent)
    {
        _alreadyVerifiedCache = Services.GetService<IAlreadyVerifiedCache>();
    }

    public AudioFileVerifier(IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(parent, gameEngine, settings, serviceProvider)
    {
        _alreadyVerifiedCache = serviceProvider.GetService<IAlreadyVerifiedCache>();
    }

    public override string FriendlyName => "Audio File format";

    public override void Verify(AudioFileInfo sampleInfo, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        var cached = _alreadyVerifiedCache?.GetEntry(sampleInfo.SampleName);

        if (cached?.AlreadyVerified is true)
        {
            if (!cached.Value.AssetExists)
            {
                AddError(VerificationError.Create(
                    this,
                    VerifierErrorCodes.FileNotFound,
                    $"Audio file '{sampleInfo.SampleName}' could not be found.",
                    VerificationSeverity.Error,
                    [.. contextInfo],
                    sampleInfo.SampleName));
            }
            return;
        }


        var sampleString = sampleInfo.SampleName;
        
        using var sampleStream = Repository.TryOpenFile(sampleString.AsSpan());

        _alreadyVerifiedCache?.TryAddEntry(sampleInfo.SampleName, sampleStream is not null);

        if (sampleStream is null)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.FileNotFound,
                $"Audio file '{sampleString}' could not be found.",
                VerificationSeverity.Error, 
                [..contextInfo],
                sampleString));
            return;
        }

        if (sampleInfo.ExpectedType == AudioFileType.Mp3)
        {
            // TODO: MP3 support to be implemented
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
                $"Audio file '{sampleString}' has an invalid format '{format}'. Supported is {WaveFormats.PCM}", 
                VerificationSeverity.Error,
                [..contextInfo],
                sampleString));
        }

        if (channels > 1 && !sampleInfo.IsAmbient)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.SampleNotMono, 
                $"Audio file '{sampleString}' is not mono audio.", 
                VerificationSeverity.Information,
                sampleString));
        }

        if (sampleRate > 48_000)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.InvalidSampleRate, 
                $"Audio file '{sampleString}' has a too high sample rate of {sampleRate}. Maximum is 48.000Hz.",
                VerificationSeverity.Error,
                [..contextInfo],
                sampleString));
        }

        if (bitPerSecondPerChannel > 16)
        {
            AddError(VerificationError.Create(
                this,
                VerifierErrorCodes.InvalidBitsPerSeconds, 
                $"Audio file '{sampleString}' has an invalid bit size of {bitPerSecondPerChannel}. Supported are 16bit.",
                VerificationSeverity.Error,
                [..contextInfo],
                sampleString));
        }
    }

    private enum WaveFormats
    {
        PCM = 1,
        MSADPCM = 2,
        IEEE_Float = 3,
    }
}
