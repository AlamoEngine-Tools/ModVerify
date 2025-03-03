namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;

internal ref struct AnimationInformationData
{
    public float FPS;
    public uint NumberFrames;
    public uint NumberBones;
    public uint RotationBlockSize;
    public uint TranslationBlockSize;
    public uint ScaleBlockSize;
}