using System;
using System.Diagnostics.CodeAnalysis;

namespace PG.StarWarsGame.Files.TED.Files;

public sealed record TedFileInformation : PetroglyphMegPackableFileInformation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TedFileInformation"/> class.
    /// </summary>
    /// <param name="path">The file path of the alo file.</param>
    /// <param name="isInMeg">Information whether this file info is created from a meg data entry.</param>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty.</exception>
    [SetsRequiredMembers]
    public TedFileInformation(string path, bool isInMeg) : base(path, isInMeg)
    {
    }
}