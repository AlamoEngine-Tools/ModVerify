using System;
using System.IO.Abstractions;
using System.Reflection;

namespace PG.StarWarsGame.Engine.Testing;

internal sealed class RepoOriginWriter(IFileSystem fileSystem, string originPath) : IRepoOriginWriter
{
    private readonly IFileSystem _fs = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

    public string OriginPath { get; } = originPath ?? throw new ArgumentNullException(nameof(originPath));

    public void Write(string relativePath, string content)
    {
        if (content == null) 
            throw new ArgumentNullException(nameof(content));
        
        var dst = Resolve(relativePath);
        EnsureParent(dst);
        _fs.File.WriteAllText(dst, content);
    }

    public void Write(string relativePath, byte[] content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));
        
        var dst = Resolve(relativePath);
        EnsureParent(dst);
        _fs.File.WriteAllBytes(dst, content);
    }

    public void WriteEmbedded(string relativePath, string resourceName, Assembly? source = null)
    {
        var asm = source ?? Assembly.GetCallingAssembly();
        var bytes = EmbeddedFixtures.Load(resourceName, asm);
        Write(relativePath, bytes);
    }

    public void Remove(string relativePath)
    {
        var dst = Resolve(relativePath);
        if (_fs.File.Exists(dst))
            _fs.File.Delete(dst);
    }

    public void WriteXml(string name, string content)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        
        Write(_fs.Path.Combine("Data", "XML", Normalize(name)), content);
    }

    private string Resolve(string relativePath)
    {
        return relativePath == null 
            ? throw new ArgumentNullException(nameof(relativePath)) 
            : _fs.Path.Combine(OriginPath, Normalize(relativePath));
    }

    private string Normalize(string path)
    {
        return path.Replace('\\', _fs.Path.DirectorySeparatorChar).Replace('/', _fs.Path.DirectorySeparatorChar);
    }

    private void EnsureParent(string fullPath)
    {
        var dir = _fs.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            _fs.Directory.CreateDirectory(dir!);
    }
}
