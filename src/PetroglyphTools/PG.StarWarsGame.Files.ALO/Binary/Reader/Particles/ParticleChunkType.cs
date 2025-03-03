namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Particles;

public enum ParticleChunkType
{
    Name = 0x0,
    Id = 0x1,
    Properties = 0x2,
    ColorTextureName = 0x3,
    BumpTextureName = 0x45,
    Bone = 0x202,
    BoneName = 0x203,
    Mesh = 0x400, 
    Connections = 0x600,
    Emitter = 0x700,
    Emitters = 0x800,
    Particle = 0x900,
    Animation = 0x1000,
}