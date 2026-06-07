using System.Collections.Generic;
using AET.ModVerify.Verifiers;

namespace AET.ModVerify.Reporting.Diagnostics;

/// <summary>Provides factories for findings produced while verifying audio sample files.</summary>
public static class Audio
{
    private static readonly ErrorDescriptor _fileNotFound = new(
        VerifierErrorCodes.FileNotFound, "AudioFileNotFound", VerificationSeverity.Error, "Audio");

    private static readonly ErrorDescriptor _notPcm = new(
        VerifierErrorCodes.SampleNotPCM, "SampleNotPcm", VerificationSeverity.Error, "Audio");

    private static readonly ErrorDescriptor _notMono = new(
        VerifierErrorCodes.SampleNotMono, "SampleNotMono", VerificationSeverity.Information, "Audio");

    private static readonly ErrorDescriptor _invalidSampleRate = new(
        VerifierErrorCodes.InvalidSampleRate, "InvalidSampleRate", VerificationSeverity.Error, "Audio");

    private static readonly ErrorDescriptor _invalidBitsPerSecond = new(
        VerifierErrorCodes.InvalidBitsPerSeconds, "InvalidBitsPerSecond", VerificationSeverity.Error, "Audio");

    /// <summary>Creates an error for an audio sample file that could not be found.</summary>
    public static VerificationError FileNotFound(IGameVerifierInfo verifier, string sampleName, IEnumerable<string> context)
        => _fileNotFound.Create(verifier, $"Audio file '{sampleName}' could not be found.", sampleName, context);

    /// <summary>Creates an error for an audio file that is not PCM-encoded.</summary>
    public static VerificationError NotPcm(IGameVerifierInfo verifier, string sampleName, string actualFormat, IEnumerable<string> context)
        => _notPcm.Create(verifier, $"Audio file '{sampleName}' has an invalid format '{actualFormat}'. Supported is PCM.", sampleName, context);

    /// <summary>Creates an error for a non-ambient audio file that is not mono.</summary>
    public static VerificationError NotMono(IGameVerifierInfo verifier, string sampleName, IEnumerable<string> context)
        => _notMono.Create(verifier, $"Audio file '{sampleName}' is not mono audio.", sampleName, context);

    /// <summary>Creates an error for an audio file whose sample rate exceeds the supported maximum.</summary>
    public static VerificationError InvalidSampleRate(IGameVerifierInfo verifier, string sampleName, int sampleRate, IEnumerable<string> context)
        => _invalidSampleRate.Create(verifier, $"Audio file '{sampleName}' has a too high sample rate of {sampleRate}. Maximum is 48.000Hz.", sampleName, context);

    /// <summary>Creates an error for an audio file whose bit depth exceeds the supported maximum.</summary>
    public static VerificationError InvalidBitsPerSecond(IGameVerifierInfo verifier, string sampleName, int bitsPerSecond, IEnumerable<string> context)
        => _invalidBitsPerSecond.Create(verifier, $"Audio file '{sampleName}' has an invalid bit size of {bitsPerSecond}. Supported are 16bit.", sampleName, context);
}
