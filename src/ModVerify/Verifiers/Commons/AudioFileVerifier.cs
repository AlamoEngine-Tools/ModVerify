using System;
using System.Collections.Generic;
using System.IO;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;
using System.Threading;
using AET.ModVerify.Reporting.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using AET.ModVerify.Verifiers.Caching;

namespace AET.ModVerify.Verifiers.Commons;

public class AudioFileVerifier : GameVerifier<AudioFileInfo>
{
    private readonly IAlreadyVerifiedCache? _alreadyVerifiedCache;

    public override string FriendlyName => "Audio File format";

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

    public override void Verify(AudioFileInfo sampleInfo, IReadOnlyCollection<string> contextInfo, CancellationToken token)
    {
        var cached = _alreadyVerifiedCache?.GetEntry(sampleInfo.SampleName);

        if (cached?.AlreadyVerified is true)
        {
            if (!cached.Value.AssetExists)
            {
                AddError(AudioErrors.FileNotFound(this, sampleInfo.SampleName, [.. contextInfo]));
            }
            return;
        }


        var sampleString = sampleInfo.SampleName;
        
        using var sampleStream = Repository.TryOpenFile(sampleString.AsSpan());

        _alreadyVerifiedCache?.TryAddEntry(sampleInfo.SampleName, sampleStream is not null);

        if (sampleStream is null)
        {
            AddError(AudioErrors.FileNotFound(this, sampleString, [..contextInfo]));
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
            AddError(AudioErrors.NotPcm(this, sampleString, format.ToString(), [..contextInfo]));
        }

        if (channels > 1 && !sampleInfo.IsAmbient)
        {
            AddError(AudioErrors.NotMono(this, sampleString, []));
        }

        if (sampleRate > 48_000)
        {
            AddError(AudioErrors.InvalidSampleRate(this, sampleString, sampleRate, [..contextInfo]));
        }

        if (bitPerSecondPerChannel > 16)
        {
            AddError(AudioErrors.InvalidBitsPerSecond(this, sampleString, bitPerSecondPerChannel, [..contextInfo]));
        }
    }

    private enum WaveFormats
    {
        PCM = 1,
        MSADPCM = 2,
        IEEE_Float = 3,
    }
}
