using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PG.Commons.Hashing;
using PG.StarWarsGame.Files.MTD.Data;

namespace PG.StarWarsGame.Engine.Database.GameManagers;

internal class EmptyMegaTextureDirectory : IMegaTextureDirectory, IEnumerator<MegaTextureFileIndex>
{
    public static readonly EmptyMegaTextureDirectory Instance = new();

    private EmptyMegaTextureDirectory()
    {
    }

    public IEnumerator<MegaTextureFileIndex> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 0;
    public bool Contains(Crc32 hashedName)
    {
        return false;
    }

    public bool TryGetIndex(Crc32 hashedName, [MaybeNullWhen(false)] out MegaTextureFileIndex index)
    {
        index = null;
        return false;
    }

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        return false;
    }

    public void Reset()
    {
    }

    public MegaTextureFileIndex Current => default!;

    object IEnumerator.Current => Current;
}