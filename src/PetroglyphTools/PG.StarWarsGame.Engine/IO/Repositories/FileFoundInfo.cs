using System;
using System.Diagnostics.CodeAnalysis;
using PG.StarWarsGame.Files.MEG.Data.Entries;

namespace PG.StarWarsGame.Engine.IO.Repositories;

internal readonly ref struct FileFoundInfo
{
    public bool FileFound => FilePath != ReadOnlySpan<char>.Empty || InMeg;

    public ReadOnlySpan<char> FilePath { get; }

    public MegDataEntryReference? MegDataEntryReference { get; }

    [MemberNotNullWhen(true, nameof(MegDataEntryReference))]
    public bool InMeg => MegDataEntryReference is not null;

    public FileFoundInfo(ReadOnlySpan<char> filePath)
    {
        FilePath = filePath;
    }

    public FileFoundInfo(MegDataEntryReference? megDataReference)
    {
        MegDataEntryReference = megDataReference;
    }
}