namespace AET.ModVerify.Verifiers;

public static class VerifierErrorCodes
{
    public const string InitializationError = "INIT00";

    public const string AssertValueNullOrEmpty = "ASRT00";
    public const string AssertValueOutOfRange = "ASRT01";
    public const string AssertValueInvalid = "ASRT02";

    public const string GenericExceptionErrorCode = "MV00";

    public const string FileCorrupt = "ENG00";


    public const string FileNotFound = "FILE00";
    public const string FilePathTooLong = "FILE01";
    public const string InvalidFilePath = "FILE02";

    public const string DuplicateFound = "DUP00";

    public const string SampleNotPCM = "WAV00";
    public const string SampleNotMono = "WAV01";
    public const string InvalidSampleRate = "WAV02";
    public const string InvalidBitsPerSeconds = "WAV03";

    public const string InvalidParticleName = "ALO01";

    public const string GenericXmlError = "XML00";
    public const string EmptyXmlRoot = "XML01";
    public const string InvalidXmlValue = "XML03";
    public const string MalformedXmlValue = "XML04";
    public const string MissingXmlAttribute = "XML05";
    public const string MissingXmlReference = "XML06";
    public const string XmlValueTooLong = "XML07";
    public const string XmlDataBeforeHeader = "XML08";
    public const string XmlMissingNode = "XML09";
    public const string XmlUnsupportedTag = "XML10";
}