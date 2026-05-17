using System.IO.Abstractions;
using System.Reflection;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Provides per-origin write operations against the underlying file system.</summary>
public interface IRepoOriginWriter
{
    /// <summary>Gets the file system backing this origin.</summary>
    IFileSystem FileSystem { get; }

    /// <summary>Gets the absolute path of this origin's root.</summary>
    string OriginPath { get; }

    /// <summary>Writes the text content to a file relative to the origin root.</summary>
    void Write(string relativePath, string content);

    /// <summary>Writes the binary content to a file relative to the origin root.</summary>
    void Write(string relativePath, byte[] content);

    /// <summary>Writes the bytes of an embedded resource to a file relative to the origin root.</summary>
    /// <param name="relativePath">The destination path relative to the origin root.</param>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <param name="source">The assembly that contains the resource. <see langword="null" /> uses the calling assembly.</param>
    void WriteEmbedded(string relativePath, string resourceName, Assembly? source = null);

    /// <summary>Removes a file relative to the origin root, if it exists.</summary>
    void Remove(string relativePath);

    /// <summary>Writes the XML content to <c>Data/XML/&lt;name&gt;</c> relative to the origin root.</summary>
    /// <remarks><paramref name="name"/> may contain subpath separators.</remarks>
    void WriteXml(string name, string content);
}
