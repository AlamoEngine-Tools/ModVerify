using System;
using System.Reflection;
using System.Text;
using PG.StarWarsGame.Files.MEG.Services.Builder;

namespace PG.StarWarsGame.Engine.Testing;

internal sealed class MegContentBuilder(IMegBuilder inner) : IMegContentBuilder
{
    private readonly IMegBuilder _inner = inner ?? throw new ArgumentNullException(nameof(inner));

    public IMegContentBuilder Add(string entryName, byte[] content)
    {
        if (entryName == null)
            throw new ArgumentNullException(nameof(entryName));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var result = _inner.AddBytes(content, entryName, encrypt: false);
        return !result.Added 
            ? throw new InvalidOperationException($"Failed to add MEG entry '{entryName}': {result.Status} ({result.Message ?? "no message"}).") 
            : this;
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
}
