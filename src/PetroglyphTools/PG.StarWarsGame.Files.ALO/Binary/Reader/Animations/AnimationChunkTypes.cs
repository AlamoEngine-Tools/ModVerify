namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;

public enum AnimationChunkTypes
{
    NumFrames = 0x01,
    Fps = 0x02,
    NumBones = 0x03,
    BoneName = 0x04,
    BoneIndex = 0x05,
    RotationBlockSize = 0x0b, 
    TranslationBlockSize = 0x0c,
    ScaleBlockSize = 0x0d,
    AnimationFile = 0x1000,
    AnimationInfo = 0x1001,
    BoneData = 0x1002,
    BoneDataInfo = 0x1003,
    BoneVisibility = 0x1007,
    BoneStepKeyTrack = 0x1008,
}