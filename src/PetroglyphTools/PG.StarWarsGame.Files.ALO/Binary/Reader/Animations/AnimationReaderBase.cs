using System.Collections.Generic;
using System.IO;
using System.Text;
using PG.StarWarsGame.Files.ALO.Data;
using PG.StarWarsGame.Files.ALO.Services;
using PG.StarWarsGame.Files.Binary;
using PG.StarWarsGame.Files.ChunkFiles.Binary.Model.Metadata;

namespace PG.StarWarsGame.Files.ALO.Binary.Reader.Animations;

internal abstract class AnimationReaderBase(AloLoadOptions loadOptions, Stream stream) 
    : AloFileReader<AlamoAnimation>(loadOptions, stream)
{
    public sealed override AlamoAnimation Read()
    {
        var bones = new List<AnimationBoneData>();
        AnimationInformationData info = default;

        var rootChunk = ChunkReader.ReadChunk();

        if (rootChunk is not { Type: (int)AnimationChunkTypes.AnimationFile })
            throw new BinaryCorruptedException("Unable to read animation file.");

        var actualSize = 0;

        do
        {
            var chunk = ChunkReader.ReadChunk(ref actualSize);
            ReadAnimation(chunk, ref info, bones);
            actualSize += chunk.BodySize;

        } while (actualSize < rootChunk.BodySize);

        if (actualSize != rootChunk.BodySize)
            throw new BinaryCorruptedException();

        if (info.NumberBones != bones.Count)
            throw new BinaryCorruptedException("The number of bones does not match the number of bone data.");

        return new AlamoAnimation
        {
            FPS = info.FPS,
            NumberBones = info.NumberBones,
            NumberFrames = info.NumberFrames,
            BoneData = bones,
        };
    }

    protected virtual void ReadAnimation(
        ChunkMetadata chunk,
        ref AnimationInformationData animationInformation,
        List<AnimationBoneData> bones)
    {
        switch (chunk.Type)
        {
            case (int)AnimationChunkTypes.AnimationInfo:
                ThrowIfChunkSizeTooLargeException(chunk);
                animationInformation = ReadAnimationInfo(chunk.BodySize);
                break;
            case (int)AnimationChunkTypes.BoneData:
                ReadBonesData(chunk.BodySize, bones);
                break;
            default:
                ChunkReader.Skip(chunk.BodySize);
                break;
        }
    }

    protected virtual void ReadBoneDataCore(ChunkMetadata chunk, List<AnimationBoneData> bones)
    {
        switch (chunk.Type)
        {
            case (int)AnimationChunkTypes.BoneDataInfo:
                ThrowIfChunkSizeTooLargeException(chunk);
                ReadBoneInfo(chunk.BodySize, bones);
                break;
            default:
                ChunkReader.Skip(chunk.BodySize);
                break;
        }
    }

    private void ReadBonesData(int chunkSize, List<AnimationBoneData> bones)
    {
        var actualSize = 0;

        do
        {
            var chunk = ChunkReader.ReadChunk(ref actualSize);
            ReadBoneDataCore(chunk, bones);
            actualSize += chunk.BodySize;

        } while (actualSize < chunkSize);

        if (actualSize != chunkSize)
            throw new BinaryCorruptedException("Unable to read particle");
    }

    private void ReadBoneInfo(int chunkSize, List<AnimationBoneData> bones)
    {
        var actualSize = 0;

        string name = null!;
        uint index = 0;

        do
        {
            var chunk = ChunkReader.ReadMiniChunk(ref actualSize);

            switch (chunk.Type)
            {
                case (int)AnimationChunkTypes.BoneName:
                    name = ChunkReader.ReadString(chunk.BodySize, Encoding.ASCII, true, ref actualSize);
                    break;
                case (int)AnimationChunkTypes.BoneIndex:
                    index = ChunkReader.ReadDword(ref actualSize);
                    break;
                default:
                    ChunkReader.Skip(chunk.BodySize, ref actualSize);
                    break;
            }

        } while (actualSize < chunkSize);

        if (actualSize != chunkSize)
            throw new BinaryCorruptedException("Unable to read particle");

        bones.Add(new AnimationBoneData(index, name));
    }

    private AnimationInformationData ReadAnimationInfo(int chunkSize)
    {
        var actualSize = 0;

        var info = new AnimationInformationData();
        do
        {
            var chunk = ChunkReader.ReadMiniChunk(ref actualSize);

            switch (chunk.Type)
            {
                case (int)AnimationChunkTypes.NumFrames:
                    info.NumberFrames = ChunkReader.ReadDword(ref actualSize);
                    break;
                case (int)AnimationChunkTypes.Fps:
                    info.FPS = ChunkReader.ReadFloat(ref actualSize);
                    break;
                case (int)AnimationChunkTypes.NumBones:
                    info.NumberBones = ChunkReader.ReadDword(ref actualSize);
                    break;
                case (int)AnimationChunkTypes.TranslationBlockSize:
                    info.TranslationBlockSize = ChunkReader.ReadDword(ref actualSize);
                    break;
                case (int)AnimationChunkTypes.RotationBlockSize:
                    info.RotationBlockSize = ChunkReader.ReadDword(ref actualSize);
                    break;
                case (int)AnimationChunkTypes.ScaleBlockSize:
                    info.ScaleBlockSize = ChunkReader.ReadDword(ref actualSize);
                    break;
                default:
                    ChunkReader.Skip(chunk.BodySize, ref actualSize);
                    break;
            }

        } while (actualSize < chunkSize);

        if (actualSize != chunkSize)
            throw new BinaryCorruptedException("Unable to read animation");

        return info;
    }
}