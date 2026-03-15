using System;
using System.Buffers;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Verifiers.Commons;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Audio.Sfx;
using PG.StarWarsGame.Engine.Localization;

namespace AET.ModVerify.Verifiers.SfxEvents;

public partial class SfxEventVerifier
{
    private void VerifySamples(SfxEvent sfxEvent, CancellationToken token)
    {
        EventHandler<VerificationErrorEventArgs> errorHandler = (_, e) => AddError(e.Error);
        _audioFileVerifier.Error += errorHandler;

        try
        {
            var isAmbient = IsAmbient2D(sfxEvent);

            foreach (var codedSample in sfxEvent.AllSamples)
            {
                VerifySample(codedSample.AsSpan(), sfxEvent, isAmbient, token);
            }
        }
        finally
        {
            _audioFileVerifier.Error -= errorHandler;
        }
    }

    private void VerifySample(
        ReadOnlySpan<char> sample, 
        SfxEvent sfxEvent, 
        bool isAmbient, 
        CancellationToken token)
    {
        char[]? pooledBuffer = null;

        var buffer = sample.Length < PGConstants.MaxMegEntryPathLength
            ? stackalloc char[PGConstants.MaxMegEntryPathLength]
            : pooledBuffer = ArrayPool<char>.Shared.Rent(sample.Length);

        try
        {
            var length = PathNormalizer.Normalize(sample, buffer, SampleNormalizerOptions);
            var sampleNameBuffer = buffer.Slice(0, length);

            if (sfxEvent.IsLocalized)
            {
                foreach (var language in _languagesToVerify)
                {
                    VerifySampleLocalized(sfxEvent, sampleNameBuffer, isAmbient, language, out var localized, token);
                    if (!localized)
                    {
                        // There is no reason to continue if we failed to localize the sample name, because the verification will fail anyway
                        // and we want to avoid multiple errors for the same sample.
                        return;
                    }
                }
            }
            else
            {
                var audioInfo = new AudioFileInfo(sampleNameBuffer.ToString(), AudioFileType.Wav, isAmbient);
                _audioFileVerifier.Verify(audioInfo, [sfxEvent.Name], token);
            }
        }
        finally
        {
            if (pooledBuffer is not null)
                ArrayPool<char>.Shared.Return(pooledBuffer);
        }
    }

    private void VerifySampleLocalized(SfxEvent sfxEvent, ReadOnlySpan<char> sample, bool isAmbient, LanguageType language, out bool localized, CancellationToken token)
    {
        char[]? pooledBuffer = null;

        var buffer = sample.Length < PGConstants.MaxMegEntryPathLength
            ? stackalloc char[PGConstants.MaxMegEntryPathLength]
            : pooledBuffer = ArrayPool<char>.Shared.Rent(sample.Length);
        try
        {
            var l = _languageManager.LocalizeFileName(sample, language, buffer, out localized);
            var localizedName = buffer.Slice(0, l);
            
            var audioInfo = new AudioFileInfo(localizedName.ToString(), AudioFileType.Wav, isAmbient);
            _audioFileVerifier.Verify(audioInfo, [sfxEvent.Name], token);
        }
        finally
        {
            if (pooledBuffer is not null)
                ArrayPool<char>.Shared.Return(pooledBuffer);
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
        var fileName = _fileSystem.Path.GetFileName(sfxEvent.Location.XmlFile).AsSpan();
        if (fileName.Equals("SFXEventsAmbient.xml".AsSpan(), StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrEmpty(sfxEvent.UsePresetName))
            return false;

        if (sfxEvent.UsePresetName!.StartsWith("Preset_AMB_2D", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
