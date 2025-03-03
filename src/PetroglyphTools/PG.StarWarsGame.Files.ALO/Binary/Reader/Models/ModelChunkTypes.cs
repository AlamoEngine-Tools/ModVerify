namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Models;

public enum ModelChunkTypes
{
    Name = 0x0,
    Id = 0x1,
    Skeleton = 0x200,
    BoneCount = 0x201,
    Bone = 0x202,
    BoneName = 0x203,
    Mesh = 0x400,
    Connections = 0x600,
    ProxyConnection = 0x603,
    Particle = 0x900,
    Animation = 0x1000,
    SubMeshMaterialInformation = 0x00010100,
    ShaderFileName = 0x00010101,
    ShaderTexture = 0x00010105,
}