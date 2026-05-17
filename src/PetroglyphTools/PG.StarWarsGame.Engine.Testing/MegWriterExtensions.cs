using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using PG.StarWarsGame.Files.MEG.Data;
using PG.StarWarsGame.Files.MEG.Files;
using PG.StarWarsGame.Files.MEG.Services;

namespace PG.StarWarsGame.Engine.Testing;

/// <summary>Provides MEG-archive write extensions for <see cref="IRepoOriginWriter"/>.</summary>
public static class MegWriterExtensions
{
    /// <summary>Writes a MEG archive composed via the configure callback.</summary>
    /// <param name="writer">The origin writer.</param>
    /// <param name="relativePath">The destination path relative to the origin root.</param>
    /// <param name="megService">The MEG file service used to create the archive.</param>
    /// <param name="configure">The callback that populates the archive entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/>, <paramref name="relativePath"/>, <paramref name="megService"/>, or <paramref name="configure"/> is <see langword="null"/>.</exception>
    public static void WriteMeg(
        this IRepoOriginWriter writer,
        string relativePath,
        IMegFileService megService,
        Action<IMegContentBuilder> configure)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));
        if (relativePath == null)
            throw new ArgumentNullException(nameof(relativePath));
        if (megService == null)
            throw new ArgumentNullException(nameof(megService));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new MegContentBuilder();
        configure(builder);
        writer.Write(relativePath, BuildArchiveBytes(writer.FileSystem, megService, builder.Entries));
    }

    /// <summary>Writes an empty MEG archive.</summary>
    /// <param name="writer">The origin writer.</param>
    /// <param name="relativePath">The destination path relative to the origin root.</param>
    /// <param name="megService">The MEG file service used to create the archive.</param>
    /// <exception cref="ArgumentNullException"><paramref name="writer"/>, <paramref name="relativePath"/>, or <paramref name="megService"/> is <see langword="null"/>.</exception>
    public static void WriteEmptyMeg(
        this IRepoOriginWriter writer,
        string relativePath,
        IMegFileService megService)
    {
        WriteMeg(writer, relativePath, megService, _ => { });
    }

    private static byte[] BuildArchiveBytes(
        IFileSystem fs,
        IMegFileService megService,
        IReadOnlyList<MegContentBuilder.PendingEntry> entries)
    {
        var stagingDir = fs.Path.Combine(fs.Path.GetTempPath(),
            $"PG.StarWarsGame.Engine.Testing.MegStage.{Guid.NewGuid():N}");
        fs.Directory.CreateDirectory(stagingDir);
        try
        {
            var builderInfos = new List<MegDataEntryBuilderInfo>(entries.Count);
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var entryFile = fs.Path.Combine(stagingDir, $"entry-{i:D6}.bin");
                fs.File.WriteAllBytes(entryFile, entry.Content);
                var fileInfo = fs.FileInfo.New(entryFile);
                builderInfos.Add(MegDataEntryBuilderInfo.FromFile(fileInfo, entry.NormalizedName, encrypt: false));
            }

            var archivePath = fs.Path.Combine(stagingDir, "archive.meg");
            using (var stream = fs.FileStream.New(archivePath, FileMode.Create, FileAccess.Write))
            {
                megService.CreateMegArchive(stream, MegFileVersion.V1, encryptionData: null, builderInfos);
            }
            return fs.File.ReadAllBytes(archivePath);
        }
        finally
        {
            if (fs.Directory.Exists(stagingDir))
                fs.Directory.Delete(stagingDir, recursive: true);
        }
    }
}
