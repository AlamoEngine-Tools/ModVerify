using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.Utilities;

namespace PG.StarWarsGame.Engine.IO.FileExistStrategies;

/// <summary>
/// Thin wrapper over Win32 <c>CreateFileA</c>. The OS resolves casing, so no per-component walk
/// and no path canonicalization is needed — the buffer goes through verbatim. Each call re-stats
/// from scratch (complete mediation), making this the safe default on Windows hosts.
/// </summary>
internal sealed class WindowsFileExistsStrategy(IFileSystem fileSystem) : FileExistsStrategy(fileSystem)
{
    public override bool FileExists(ReadOnlySpan<char> gameDirectory, ref ValueStringBuilder stringBuilder)
    {
        // We *could* also use the slightly faster GetFileAttributesA. However, CreateFileA and
        // GetFileAttributesA are implemented entirely independently in Windows; the game uses
        // CreateFileA, so we stick to it to remain as close to engine behavior as possible.
        // NB: GetPinnableReference(true) zero-terminates so CreateFileA gets a valid C string.
        var fileHandle = CreateFile(
            in stringBuilder.GetPinnableReference(true),
            FileAccess.Read,
            FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.Normal,
            IntPtr.Zero);

        return IsValidAndClose(fileHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidAndClose(IntPtr handle)
    {
        var isValid = handle != IntPtr.Zero && handle != new IntPtr(-1);
        if (isValid)
            CloseHandle(handle);
        return isValid;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        in char lpFileName,
        [MarshalAs(UnmanagedType.U4)] FileAccess access,
        [MarshalAs(UnmanagedType.U4)] FileShare share,
        IntPtr securityAttributes,
        [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
        [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);
}
