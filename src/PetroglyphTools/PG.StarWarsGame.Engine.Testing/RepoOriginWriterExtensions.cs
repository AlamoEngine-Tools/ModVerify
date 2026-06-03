using System;
using System.Reflection;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Provides convenience extensions for <see cref="IRepoOriginWriter"/>.</summary>
public static class RepoOriginWriterExtensions
{
    /// <summary>Writes every embedded resource whose name starts with <paramref name="resourcePrefix"/> under the origin, stripping the prefix to form the destination path.</summary>
    /// <param name="writer">The origin writer.</param>
    /// <param name="resourcePrefix">The resource name prefix that identifies a tree of fixtures (e.g., <c>"MinimalFoc"</c>).</param>
    /// <param name="source">The assembly containing the resources. <see langword="null" /> uses the calling assembly.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/> or <paramref name="resourcePrefix"/> is <see langword="null"/>.</exception>
    public static void WriteEmbeddedTree(this IRepoOriginWriter writer, string resourcePrefix, Assembly? source = null)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        if (resourcePrefix == null)
            throw new ArgumentNullException(nameof(resourcePrefix));

        var asm = source ?? Assembly.GetCallingAssembly();
        var prefix = resourcePrefix.TrimEnd('/') + "/";
        foreach (var name in asm.GetManifestResourceNames())
        {
            if (!name.StartsWith(prefix, StringComparison.Ordinal))
                continue;
            writer.WriteEmbedded(name.Substring(prefix.Length), name, asm);
        }
    }
}
