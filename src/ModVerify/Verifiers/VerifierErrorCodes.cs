namespace AET.ModVerify.Verifiers;

public static class VerifierErrorCodes
{
    public const string GenericExceptionErrorCode = "MV00";

    public const string DuplicateFound = "DUP00";

    public const string SampleNotFound = "WAV00";
    public const string FilePathTooLong = "WAV01";
    public const string SampleNotPCM = "WAV02";
    public const string SampleNotMono = "WAV03";
    public const string InvalidSampleRate = "WAV04";
    public const string InvalidBitsPerSeconds = "WAV05";

    public const string ModelNotFound = "ALO00";
    public const string ModelBroken = "ALO01";
    public const string ModelMissingTexture = "ALO02";
    public const string ModelMissingProxy = "ALO03";
    public const string ModelMissingShader = "ALO04";
    public const string InvalidTexture = "ALO05";
    public const string InvalidShader = "ALO06";
    public const string InvalidProxy = "ALO07";
    public const string InvalidParticleName = "ALO08";
}