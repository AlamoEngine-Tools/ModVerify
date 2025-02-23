namespace PG.StarWarsGame.Engine.Rendering;

public enum PrimRenderMode
{
    PrimOpaque = 0x0,
    PrimAdditive = 0x1,
    PrimAlpha = 0x2,
    PrimModulate = 0x3,
    PrimDepthspriteAdditive = 0x4,
    PrimDepthspriteAlpha = 0x5,
    PrimDepthspriteModulate = 0x6,
    PrimDiffuseAlpha = 0x7,
    PrimStencilDarken = 0x8,
    PrimStencilDarkenBlur = 0x9,
    PrimHeat = 0xa,
    PrimParticleBumpAlpha = 0xb,
    PrimDecalBumpAlpha = 0xc,
    PrimAlphaScanlines = 0xd,
    PrimCount = 0xe
}