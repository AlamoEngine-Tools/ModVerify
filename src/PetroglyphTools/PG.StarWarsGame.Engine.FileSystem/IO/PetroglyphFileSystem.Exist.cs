using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PG.StarWarsGame.Engine.Utilities;
using System.IO;
#if NETSTANDARD2_0
using AnakinRaW.CommonUtilities.FileSystem;
#endif

namespace PG.StarWarsGame.Engine.IO;

public sealed partial class PetroglyphFileSystem
{
    internal bool FileExists(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder, ReadOnlySpan<char> gameDirectory)
    {
        stringBuilder.Length = 0;

        if (IsPathFullyQualified_Exists(filePath))
            stringBuilder.Append(filePath);
        else
            JoinPath(gameDirectory, filePath, ref stringBuilder);
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            NormalizePath(ref stringBuilder);
            
            var actualFilePath = stringBuilder.AsSpan();
            return FileExistsCaseInsensitive(actualFilePath, ref stringBuilder, gameDirectory.Length);
        }

        // We *could* also use the slightly faster GetFileAttributesA.
        // However, CreateFileA and GetFileAttributesA are implemented complete independent.
        // The game uses CreateFileA.
        // Thus, we should stick to what the game uses in order to be as close to the engine as possible
        // NB: It's also important that the string builder is zero-terminated, as otherwise CreateFileA might get invalid data.
        var fileHandle = CreateFile(
            in stringBuilder.GetPinnableReference(true),
            FileAccess.Read,
            FileShare.Read,
            IntPtr.Zero,
            FileMode.Open,
            FileAttributes.Normal, IntPtr.Zero);
            
        return IsValidAndClose(fileHandle);
    }
    
    // NB: This method assumes backslashes have been normalized to forward slashes
    // NB: This method operates on the actual file system
    private bool FileExistsCaseInsensitive(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder)
    {
        Debug.Assert(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
        
        var pathString = filePath.ToString();
        if (_underlyingFileSystem.File.Exists(pathString))
            return true;

        var directory = _underlyingFileSystem.Path.GetDirectoryName(pathString);
        var fileName = _underlyingFileSystem.Path.GetFileName(pathString);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
            return false;

        if (!_underlyingFileSystem.Directory.Exists(directory))
        {
            if (!FileExistsCaseInsensitive(directory.AsSpan(), ref stringBuilder))
                return false;

            directory = stringBuilder.AsSpan().ToString();
        }

        var files = _underlyingFileSystem.Directory.GetFiles(directory);
        var directories = _underlyingFileSystem.Directory.GetDirectories(directory);

        foreach (var file in files)
        {
            var name = _underlyingFileSystem.Path.GetFileName(file);
            if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(file);
                return true;
            }
        }

        foreach (var dir in directories)
        {
            var name = _underlyingFileSystem.Path.GetFileName(dir);
            if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(dir);
                return true;
            }
        }

        return false;
    }

    private bool IsPathFullyQualified_Exists(ReadOnlySpan<char> path)
    {
        // This is really tricky, because under Windows "/" or "\" do NOT
        // indicate a fully qualified path, under Linux however "/" does. 
        // The PGFileSystem is implemented to treat backslashes as directory separators.
        // However, this must not happen here, since we are operating on the actual file system.
        // E.g, \\Data\\Art\\... MUST not be treated as a fully qualified path
        // This means, ultimately, we can just delegate to the underlying file system.

        return _underlyingFileSystem.Path.IsPathFullyQualified(path);
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




    /// <summary>
    /// Checks whether a file exists using case-insensitive path resolution.
    /// On success, <paramref name="stringBuilder"/> contains the actual on-disk path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Strategy:
    /// 1. Fast path: single stat() for exact-case match.
    /// 2. Find deepest existing directory prefix, starting from the hint position.
    ///    With a correct hint this costs 1 stat. Without a hint (or bad hint),
    ///    walks backward — graceful degradation, never throws.
    /// 3. Forward resolve: lazily enumerate only the mismatched components.
    /// </para>
    /// <para>
    /// No exceptions occur in normal flow: Directory.Exists returns bool,
    /// and we only enumerate directories whose existence has been confirmed.
    /// </para>
    /// </remarks>
    /// <param name="filePath">
    /// Normalized absolute path with forward slashes. May alias stringBuilder's buffer.
    /// </param>
    /// <param name="stringBuilder">
    /// On success, overwritten with the actual on-disk path.
    /// </param>
    /// <param name="knownGoodPrefixLength">
    /// Length of the path prefix known to exist with correct casing (typically gameDirectory.Length).
    /// Pass 0 if unknown — the method falls back to a backward walk.
    /// </param>
    private bool FileExistsCaseInsensitive(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder, int knownGoodPrefixLength)
    {
        Debug.Assert(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        var pathString = filePath.ToString();

        // Fast path: exact case match — single stat() syscall
        if (_underlyingFileSystem.File.Exists(pathString))
            return true;

        if (pathString.Length == 0)
            return false;

        
        var path = pathString.AsSpan();

        // Pre-resolve "." and ".." segments in the path
        pathString = ResolveDotSegments(pathString);
        path = pathString.AsSpan();

        var rootLen = path[0] == '/' ? 1 : 0;
        var resolvedEnd = rootLen;

        int searchEnd;
        if (knownGoodPrefixLength > 0)
        {
            searchEnd = knownGoodPrefixLength;
            while (searchEnd > 1 && path[searchEnd - 1] == '/')
                searchEnd--;
        }
        else
        {
            var lastSlash = path.LastIndexOf('/');
            searchEnd = lastSlash >= 0 ? (lastSlash == 0 ? 1 : lastSlash) : 0;
        }

        // Walk backward until we find an existing directory.
        // Save the successful prefix string to reuse as the first currentDir.
        string? resolvedPrefix = null;
        while (searchEnd > resolvedEnd)
        {
            var prefix = pathString.Substring(0, searchEnd);
            if (_underlyingFileSystem.Directory.Exists(prefix))
            {
                resolvedEnd = searchEnd;
                resolvedPrefix = prefix;
                break;
            }

            var slash = path.Slice(0, searchEnd).LastIndexOf('/');
            if (slash < 0)
                break;
            searchEnd = slash == 0 ? 1 : slash;
        }

        if (resolvedEnd == 0)
            return false;

        // Reuse the prefix from Directory.Exists if available, otherwise allocate once.
        var currentDir = resolvedPrefix ?? pathString.Substring(0, resolvedEnd);

        // Save original content so we can restore on failure
        var originalContent = stringBuilder.AsSpan().ToString();
        stringBuilder.Length = 0;
        stringBuilder.Append(currentDir);

        var pos = resolvedEnd;
        if (pos < path.Length && path[pos] == '/')
            pos++;

        while (pos < path.Length)
        {
            var nextSlash = path.Slice(pos).IndexOf('/');
            var componentEnd = nextSlash >= 0 ? pos + nextSlash : path.Length;
            var component = path.Slice(pos, componentEnd - pos);

            if (component.IsEmpty)
            {
                pos = componentEnd + 1;
                continue;
            }

            // Handle "." (current directory) segment
            if (component.Length == 1 && component[0] == '.')
            {
                pos = componentEnd + 1;
                continue;
            }

            // Handle ".." (parent directory) segment
            if (component.Length == 2 && component[0] == '.' && component[1] == '.')
            {
                var curDir = stringBuilder.AsSpan();
                var lastSlash = curDir.LastIndexOf('/');
                if (lastSlash > rootLen)
                {
                    currentDir = curDir.Slice(0, lastSlash).ToString();
                    stringBuilder.Length = 0;
                    stringBuilder.Append(currentDir);
                }
                else if (lastSlash == rootLen && rootLen > 0)
                {
                    currentDir = curDir.Slice(0, rootLen).ToString();
                    stringBuilder.Length = 0;
                    stringBuilder.Append(currentDir);
                }
                pos = componentEnd + 1;
                continue;
            }

            var isLast = componentEnd >= path.Length;

            var entries = isLast
                ? _underlyingFileSystem.Directory.EnumerateFiles(currentDir)
                : _underlyingFileSystem.Directory.EnumerateDirectories(currentDir);

            var found = false;
            foreach (var entry in entries)
            {
                if (_underlyingFileSystem.Path.GetFileName(entry.AsSpan()).Equals(component, StringComparison.OrdinalIgnoreCase))
                {
                    stringBuilder.Length = 0;
                    stringBuilder.Append(entry);
                    currentDir = entry; // entry is already a string — reuse it
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(originalContent);
                return false;
            }

            pos = componentEnd + 1;
        }

        return true;
    }

    private static bool ContainsDotSegment(string path)
    {
        // Check for "." or ".." as standalone path segments.
        // Segments are delimited by '/' or string boundaries.
        var span = path.AsSpan();
        var pos = 0;
        while (pos < span.Length)
        {
            var nextSlash = span.Slice(pos).IndexOf('/');
            var end = nextSlash >= 0 ? pos + nextSlash : span.Length;
            var len = end - pos;
            if (len == 1 && span[pos] == '.')
                return true;
            if (len == 2 && span[pos] == '.' && span[pos + 1] == '.')
                return true;
            pos = end + 1;
        }
        return false;
    }

    private static string ResolveDotSegments(string path)
    {
        if (!ContainsDotSegment(path))
            return path;

        var segments = path.Split('/');
        var stack = new System.Collections.Generic.List<string>(segments.Length);

        foreach (var seg in segments)
        {
            if (seg == ".")
                continue;

            if (seg == "..")
            {
                // Don't pop past root (empty first segment for absolute paths)
                if (stack.Count > 0 && stack[stack.Count - 1] != "" && stack[stack.Count - 1] != "..")
                    stack.RemoveAt(stack.Count - 1);
            }
            else
            {
                stack.Add(seg);
            }
        }

        return string.Join("/", stack);
    }
    
    /// <summary>
    /// Wine-style case-insensitive file existence check.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mirrors <c>lookup_unix_name</c> + <c>find_file_in_dir</c> in Wine's
    /// <c>dlls/ntdll/unix/file.c</c>: walk path components forward starting from
    /// a known-good prefix; for each component, try an exact-case stat as a fast
    /// path before falling back to a single directory enumeration. Components
    /// whose case already matches on disk cost one stat each; only the
    /// mismatched components incur an enumeration.
    /// </para>
    /// <para>
    /// Differs from the backward-walking variant: there's no "find deepest
    /// existing prefix" probe. Wine trusts the prefix and walks forward
    /// component by component, which avoids backward stats but costs one extra
    /// stat per correctly-cased trailing component versus the backward walk
    /// when many trailing components are missing.
    /// </para>
    /// </remarks>
    /// <param name="filePath">
    /// Normalized absolute path with forward slashes. May alias <paramref name="stringBuilder"/>'s buffer.
    /// </param>
    /// <param name="stringBuilder">
    /// On success, overwritten with the actual on-disk path.
    /// </param>
    /// <param name="knownGoodPrefixLength">
    /// Length of the path prefix known to exist with correct casing (typically gameDirectory.Length).
    /// Wine's analogue is <c>root_fd</c>: the prefix is trusted, never re-validated component-wise.
    /// Pass 0 to start from the filesystem root ("/").
    /// </param>
    private bool FileExistsCaseInsensitiveWine(ReadOnlySpan<char> filePath, ref ValueStringBuilder stringBuilder, int knownGoodPrefixLength)
    {
        Debug.Assert(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        var pathString = filePath.ToString();

        // Top-level shortcut: matches Wine's first fstatat over the full nt_name.
        if (_underlyingFileSystem.File.Exists(pathString))
            return true;

        if (pathString.Length == 0)
            return false;

        var path = pathString.AsSpan();
        var rootLen = path[0] == '/' ? 1 : 0;

        int prefixEnd;
        if (knownGoodPrefixLength > rootLen)
        {
            prefixEnd = Math.Min(knownGoodPrefixLength, path.Length);
            while (prefixEnd > rootLen && path[prefixEnd - 1] == '/')
                prefixEnd--;
        }
        else
        {
            prefixEnd = rootLen;
        }

        if (prefixEnd == 0)
            return false;

        var currentDir = pathString.Substring(0, prefixEnd);
        if (!_underlyingFileSystem.Directory.Exists(currentDir))
            return false;

        stringBuilder.Length = 0;
        stringBuilder.Append(currentDir);

        var pos = prefixEnd;
        if (pos < path.Length && path[pos] == '/')
            pos++;

        while (pos < path.Length)
        {
            var rest = path.Slice(pos);
            var nextSlash = rest.IndexOf('/');
            var componentEnd = nextSlash >= 0 ? pos + nextSlash : path.Length;
            var component = path.Slice(pos, componentEnd - pos);

            if (component.IsEmpty)
            {
                pos = componentEnd + 1;
                continue;
            }

            var isLast = componentEnd >= path.Length;

            // Wine's per-component shortcut: append the literal component and
            // stat. For non-last components require a directory; for the leaf
            // require a file (Wine's fstatat is type-agnostic, but our public
            // contract is FileExists, so we narrow at the leaf).
            var savedLen = stringBuilder.Length;
            if (savedLen == 0 || stringBuilder[savedLen - 1] != '/')
                stringBuilder.Append('/');
            stringBuilder.Append(component);

            var literalPath = stringBuilder.AsSpan().ToString();
            if (isLast)
            {
                if (_underlyingFileSystem.File.Exists(literalPath))
                    return true;
            }
            else if (_underlyingFileSystem.Directory.Exists(literalPath))
            {
                currentDir = literalPath;
                pos = componentEnd + 1;
                continue;
            }

            // Literal stat missed; roll back and enumerate the parent directory.
            stringBuilder.Length = savedLen;

            var entries = isLast
                ? _underlyingFileSystem.Directory.EnumerateFiles(currentDir)
                : _underlyingFileSystem.Directory.EnumerateDirectories(currentDir);

            var found = false;
            foreach (var entry in entries)
            {
                if (_underlyingFileSystem.Path.GetFileName(entry.AsSpan()).Equals(component, StringComparison.OrdinalIgnoreCase))
                {
                    stringBuilder.Length = 0;
                    stringBuilder.Append(entry);
                    currentDir = entry;
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;

            if (isLast)
                return true;

            pos = componentEnd + 1;
        }

        return true;
    }
}