﻿namespace PG.StarWarsGame.Files.ChunkFiles.Binary.Metadata;

public enum ChunkType
{
    Unknown,
    Name = 0x0,
    Id = 0x1,
    Persistance = 0x2,
    Properties = 0x2,
    ColorTextureName = 0x3,
    BumpTextureName = 0x45,
    Skeleton = 0x200,
    BoneCount = 0x201,
    Bone = 0x202,
    BoneName = 0x203,
    Mesh = 0x400,
    MeshName = 0x401,
    MeshInformation = 0x402,
    Connections = 0x600,
    ConnectionCounts = 0x601,
    ObjectConnection = 0x602,
    ProxyConnection = 0x603,
    Dazzle = 0x604,
    Emitter = 0x700,
    Emitters = 0x800,
    Particle = 0x900,
    Animation = 0x1000,
    AnimationInformation = 0x1001,
    AnimationBoneData = 0x1002,
    Light = 0x1300,
    ParticleUaW = 0x1500,
    SubMeshData = 0x00010000,
    SubMeshMaterialInformation = 0x00010100,
    ShaderFileName = 0x00010101,
    ShaderTexture = 0x00010105,
}