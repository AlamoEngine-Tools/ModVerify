using System;
using System.IO;
using System.Reflection;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Loads bytes from embedded resources.</summary>
public static class EmbeddedFixtures
{
    /// <summary>Returns the bytes of an embedded resource.</summary>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <param name="source">The assembly that contains the resource. <see langword="null" /> uses the calling assembly.</param>
    /// <exception cref="InvalidOperationException"><paramref name="resourceName"/> was not found in <paramref name="source"/>.</exception>
    public static byte[] Load(string resourceName, Assembly? source = null)
    {
        if (resourceName == null)
            throw new ArgumentNullException(nameof(resourceName));
        
        var asm = source ?? Assembly.GetCallingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found in {asm.FullName}.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
