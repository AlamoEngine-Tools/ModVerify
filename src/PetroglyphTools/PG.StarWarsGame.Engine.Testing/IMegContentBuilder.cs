using System.Reflection;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Composes the entries of a MEG archive.</summary>
public interface IMegContentBuilder
{
    /// <summary>Adds an entry with binary content.</summary>
    /// <remarks>The entry name is normalized to canonical MEG form (uppercase, backslash-separated).</remarks>
    IMegContentBuilder Add(string entryName, byte[] content);

    /// <summary>Adds an entry with text content.</summary>
    /// <remarks>Encoded as UTF-8. The entry name is normalized to canonical MEG form.</remarks>
    IMegContentBuilder Add(string entryName, string content);

    /// <summary>Adds an entry whose content is loaded from an embedded resource.</summary>
    /// <param name="entryName">The entry name (will be normalized).</param>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <param name="source">The assembly containing the resource. <see langword="null" /> uses the calling assembly.</param>
    IMegContentBuilder AddEmbedded(string entryName, string resourceName, Assembly? source = null);
}
