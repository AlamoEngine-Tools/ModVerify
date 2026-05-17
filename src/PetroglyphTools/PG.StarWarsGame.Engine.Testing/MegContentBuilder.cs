using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PG.StarWarsGame.Files.MEG.Services.Builder.Normalization;

namespace PG.StarWarsGame.Engine.Testing;

internal sealed class MegContentBuilder : IMegContentBuilder
{
    private static readonly EmpireAtWarMegDataEntryPathNormalizer Normalizer = new();

    private readonly List<PendingEntry> _entries = new();

    public IReadOnlyList<PendingEntry> Entries => _entries;

    public IMegContentBuilder Add(string entryName, byte[] content)
    {
        if (entryName == null)
            throw new ArgumentNullException(nameof(entryName));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        _entries.Add(new PendingEntry(Normalize(entryName), content));
        return this;
    }

    public IMegContentBuilder Add(string entryName, string content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        return Add(entryName, Encoding.UTF8.GetBytes(content));
    }

    public IMegContentBuilder AddEmbedded(string entryName, string resourceName, Assembly? source = null)
    {
        var asm = source ?? Assembly.GetCallingAssembly();
        return Add(entryName, EmbeddedFixtures.Load(resourceName, asm));
    }

    private static string Normalize(string entryName)
    {
        return Normalizer.Normalize(entryName);
    }

    internal readonly struct PendingEntry(string normalizedName, byte[] content)
    {
        public string NormalizedName { get; } = normalizedName;
        public byte[] Content { get; } = content;
    }
}
