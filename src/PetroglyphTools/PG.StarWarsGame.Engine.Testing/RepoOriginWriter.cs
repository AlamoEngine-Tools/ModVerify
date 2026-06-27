using System;
using System.IO.Abstractions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Files.MEG.Data;
using PG.StarWarsGame.Files.MEG.Files;
using PG.StarWarsGame.Files.MEG.Services.Builder;

namespace PG.StarWarsGame.Engine.Testing;

internal sealed class RepoOriginWriter(IServiceProvider services, string originPath, Action<string> registerMeg) : IRepoOriginWriter
{
    private readonly IServiceProvider _services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly IFileSystem _fileSystem = services.GetRequiredService<IFileSystem>();
    private readonly string _originPath = originPath ?? throw new ArgumentNullException(nameof(originPath));
    private readonly Action<string> _registerMeg = registerMeg ?? throw new ArgumentNullException(nameof(registerMeg));

    public void Write(string relativePath, string content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var dst = Resolve(relativePath);
        EnsureParent(dst);
        _fileSystem.File.WriteAllText(dst, content);
    }

    public void Write(string relativePath, byte[] content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var dst = Resolve(relativePath);
        EnsureParent(dst);
        _fileSystem.File.WriteAllBytes(dst, content);
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
        if (_fileSystem.File.Exists(dst))
            _fileSystem.File.Delete(dst);
    }

    public void WriteXml(string name, string content)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        Write(_fileSystem.Path.Combine("Data", "XML", Normalize(name)), content);
    }

    public void WriteMeg(string relativePath, Action<IMegContentBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var dst = Resolve(relativePath);
        EnsureParent(dst);

        using var megBuilder = new EmpireAtWarMegBuilder(_fileSystem.Path.GetDirectoryName(dst)!, _services);
        configure(new MegContentBuilder(megBuilder));
        using var fileInfo = new MegFileInformation(dst, MegVersion.V1, encryptionData: null);
        megBuilder.Build(fileInfo, overwrite: true);
    }

    public void WriteEmptyMeg(string relativePath)
    {
        WriteMeg(relativePath, _ => { });
    }

    public void RegisterAndWriteMeg(string relativePath, Action<IMegContentBuilder> configure)
    {
        WriteMeg(relativePath, configure);
        _registerMeg(relativePath);
    }

    private string Resolve(string relativePath)
    {
        return relativePath == null
            ? throw new ArgumentNullException(nameof(relativePath))
            : _fileSystem.Path.Combine(_originPath, Normalize(relativePath));
    }

    private string Normalize(string path)
    {
        return path.Replace('\\', _fileSystem.Path.DirectorySeparatorChar).Replace('/', _fileSystem.Path.DirectorySeparatorChar);
    }

    private void EnsureParent(string fullPath)
    {
        var dir = _fileSystem.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            _fileSystem.Directory.CreateDirectory(dir!);
    }
}
