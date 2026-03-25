namespace AET.ModVerify.Verifiers.Commons;

public class AudioFileInfo
{
    public string SampleName { get; }
    public AudioFileType ExpectedType { get; }
    public bool IsAmbient { get; }

    public AudioFileInfo(string sampleName, AudioFileType expectedType, bool isAmbient)
    {
        SampleName = sampleName;
        ExpectedType = expectedType;
        IsAmbient = isAmbient;
    }
}
