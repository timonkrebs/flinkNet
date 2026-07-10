/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Text;
using System.Text.RegularExpressions;
using FlinkNet.Annotations;
using FlinkNet.Core.Memory;
using FlinkNet.Util;

namespace FlinkNet.Core.Fs;

/// <summary>
/// The components of the hierarchical URI underlying a <see cref="Path"/>.
///
/// <para>PORT NOTE: Java's <c>Path.toUri()</c> returns a <c>java.net.URI</c>; .NET's
/// <c>System.Uri</c> cannot represent scheme-less relative hierarchical URIs faithfully, so the
/// port exposes the components directly.</para>
/// </summary>
[Public]
public sealed record PathUri(string? Scheme, string? Authority, string UriPath);

/// <summary>
/// Names a file or directory in a <see cref="FileSystem"/>. Path strings use slash as the
/// directory separator. A path string is absolute if it begins with a slash.
///
/// <para>Tailing slashes are removed from the path string, double slashes collapse, backslashes
/// normalize to slashes, and Windows drive paths receive a leading slash internally — matching
/// Java's behavior.</para>
/// </summary>
[Public]
public class Path : IComparable<Path>
{
    /// <summary>The directory separator, a slash.</summary>
    public const string Separator = "/";

    /// <summary>The directory separator, a slash, as a character.</summary>
    public const char SeparatorChar = '/';

    /// <summary>Character denoting the current directory.</summary>
    public const string CurDir = ".";

    private readonly string? _scheme;
    private readonly string? _authority;
    private readonly string _path;

    // ------------------------------------------------------------------------

    /// <summary>
    /// Constructs a path object from a given URI components.
    /// </summary>
    public Path(PathUri uri)
        : this(uri.Scheme, uri.Authority, uri.UriPath)
    {
    }

    /// <summary>Resolve a child path against a parent path.</summary>
    public Path(string parent, string child)
        : this(new Path(parent), new Path(child))
    {
    }

    /// <summary>Resolve a child path against a parent path.</summary>
    public Path(Path parent, string child)
        : this(parent, new Path(child))
    {
    }

    /// <summary>Resolve a child path against a parent path.</summary>
    public Path(string parent, Path child)
        : this(new Path(parent), child)
    {
    }

    /// <summary>Resolve a child path against a parent path.</summary>
    public Path(Path parent, Path child)
    {
        // an absolute child URI (with its own scheme) wins entirely, like URI.resolve
        if (child._scheme is not null)
        {
            _scheme = child._scheme;
            _authority = child._authority;
            string childOwnPath = NormalizePath(child._path);
            _path = NormalizeSegments(childOwnPath, childOwnPath.StartsWith('/'));
            return;
        }

        string childPath = child._path.StartsWith(Separator, StringComparison.Ordinal)
            ? child._path[1..]
            : child._path;

        string parentPath = parent._path;
        string merged =
            parentPath is "/" or "" ? "/" + childPath : parentPath + "/" + childPath;

        _scheme = parent._scheme;
        _authority = parent._authority;
        _path = NormalizeSegments(NormalizePath(merged), absolute: merged.StartsWith('/'));
    }

    private static string CheckPathArg(string path)
    {
        // disallow construction of a Path from an empty string
        if (path == null)
        {
            throw new ArgumentException("Can not create a Path from a null string");
        }
        if (path.Length == 0)
        {
            throw new ArgumentException("Can not create a Path from an empty string");
        }
        return path;
    }

    /// <summary>
    /// Construct a path from a string. Path strings are URIs, but with unescaped elements and
    /// some additional normalization.
    /// </summary>
    public Path(string pathString)
    {
        pathString = CheckPathArg(pathString);

        // add a slash in front of paths with Windows drive letters
        if (HasWindowsDrive(pathString, slashed: false))
        {
            pathString = "/" + pathString;
        }

        // parse uri components
        string? scheme = null;
        string? authority = null;

        int start = 0;

        // parse uri scheme, if any
        int colon = pathString.IndexOf(':');
        int slash = pathString.IndexOf('/');
        if (colon != -1 && (slash == -1 || colon < slash))
        {
            // has a scheme
            scheme = pathString[..colon];
            start = colon + 1;
        }

        // parse uri authority, if any
        if (pathString.AsSpan(start).StartsWith("//") && pathString.Length - start > 2)
        {
            // has authority
            int nextSlash = pathString.IndexOf('/', start + 2);
            int authEnd = nextSlash > 0 ? nextSlash : pathString.Length;
            authority = pathString[(start + 2)..authEnd];
            start = authEnd;
        }

        // uri path is the rest of the string -- query & fragment not supported
        string path = pathString[start..];

        _scheme = scheme;
        _authority = string.IsNullOrEmpty(authority) ? null : authority;
        string normalized = NormalizePath(path);
        _path = NormalizeSegments(normalized, normalized.StartsWith('/'));
    }

    /// <summary>Construct a Path from components.</summary>
    public Path(string? scheme, string? authority, string path)
    {
        path = CheckPathArg(path);
        _scheme = scheme;
        _authority = string.IsNullOrEmpty(authority) ? null : authority;
        string normalized = NormalizePath(path);
        _path = NormalizeSegments(normalized, normalized.StartsWith('/'));
    }

    /// <summary>Normalizes a path string: backslashes to slashes, collapse duplicate slashes,
    /// remove tailing separator (except for roots).</summary>
    private static string NormalizePath(string path)
    {
        // remove consecutive slashes & backslashes
        path = path.Replace('\\', '/');
        if (path.Contains("//"))
        {
            path = Regex.Replace(path, "/{2,}", "/");
        }

        // remove tailing separator
        if (path.EndsWith(Separator, StringComparison.Ordinal)
            && path != Separator // UNIX root path
            && !Regex.IsMatch(path, "^/\\p{L}+:/$")) // Windows root path
        {
            path = path[..^Separator.Length];
        }

        return path;
    }

    /// <summary>Removes "." and ".." segments, like Java's <c>URI.normalize()</c>.</summary>
    private static string NormalizeSegments(string path, bool absolute)
    {
        if (!path.Contains('.'))
        {
            return path;
        }

        string[] segments = path.Split('/');
        var result = new List<string>();
        foreach (string segment in segments)
        {
            switch (segment)
            {
                case ".":
                    break;
                case ".."
                    when result.Count > 0 && result[^1] != ".." && result[^1].Length > 0:
                    result.RemoveAt(result.Count - 1);
                    break;
                case "..":
                    if (!absolute)
                    {
                        result.Add(segment);
                    }
                    break;
                default:
                    result.Add(segment);
                    break;
            }
        }

        string joined = string.Join('/', result);
        if (absolute && !joined.StartsWith('/'))
        {
            joined = "/" + joined;
        }
        return joined.Length == 0 && absolute ? "/" : joined;
    }

    // ------------------------------------------------------------------------

    /// <summary>Returns the components of the path's hierarchical URI.</summary>
    public PathUri ToUri() => new(_scheme, _authority, _path);

    /// <summary>Returns the FileSystem that owns this Path.</summary>
    public FileSystem GetFileSystem() => FileSystem.Get(this);

    /// <summary>Checks if the directory of this path is absolute.</summary>
    public bool IsAbsolute()
    {
        int start = HasWindowsDrive(_path, slashed: true) ? 3 : 0;
        return _path.Length > start && _path[start] == SeparatorChar;
    }

    /// <summary>Returns the final component of this path, i.e., everything that follows the last
    /// separator.</summary>
    public string GetName()
    {
        int slash = _path.LastIndexOf(SeparatorChar);
        return _path[(slash + 1)..];
    }

    /// <summary>Return full path.</summary>
    public string GetPath() => _path;

    /// <summary>Returns the parent of a path, i.e., everything that precedes the last separator,
    /// or null if at root.</summary>
    public Path? GetParent()
    {
        string path = _path;
        int lastSlash = path.LastIndexOf('/');
        int start = HasWindowsDrive(path, slashed: true) ? 3 : 0;
        if (path.Length == start || (lastSlash == start && path.Length == start + 1))
        {
            // at root
            return null;
        }
        string parent;
        if (lastSlash == -1)
        {
            parent = CurDir;
        }
        else
        {
            int end = HasWindowsDrive(path, slashed: true) ? 3 : 0;
            parent = path[..(lastSlash == end ? end + 1 : lastSlash)];
        }
        return new Path(_scheme, _authority, parent);
    }

    /// <summary>Adds a suffix to the final name in the path.</summary>
    public Path Suffix(string suffix) => new(GetParent()!, GetName() + suffix);

    public override string ToString()
    {
        // we can't use uri.toString(), which escapes everything, because we want
        // illegal characters unescaped in the string, for glob processing, etc.
        var buffer = new StringBuilder();
        if (_scheme != null)
        {
            buffer.Append(_scheme).Append(':');
        }
        if (_authority != null)
        {
            buffer.Append("//").Append(_authority);
        }
        if (_path.Length > 0)
        {
            string path = _path;
            if (path.StartsWith('/')
                && HasWindowsDrive(path, slashed: true)
                && _scheme == null
                && _authority == null)
            {
                // remove slash before drive
                path = path[1..];
            }
            buffer.Append(path);
        }
        return buffer.ToString();
    }

    public override bool Equals(object? obj) =>
        obj is Path that
            && _scheme == that._scheme
            && _authority == that._authority
            && _path == that._path;

    public override int GetHashCode() => HashCode.Combine(_scheme, _authority, _path);

    public int CompareTo(Path? other) =>
        other is null ? 1 : string.CompareOrdinal(ToString(), other.ToString());

    /// <summary>Returns the number of elements in this path.</summary>
    public int Depth()
    {
        string path = _path;
        int depth = 0;
        int slash = path.Length == 1 && path[0] == '/' ? -1 : 0;
        while (slash != -1)
        {
            depth++;
            slash = path.IndexOf(Separator, slash + 1, StringComparison.Ordinal);
        }
        return depth;
    }

    /// <summary>
    /// Returns a qualified path object: relative paths resolve against the file system's working
    /// directory, and missing scheme/authority are taken from the file system's URI.
    /// </summary>
    public Path MakeQualified(FileSystem fs)
    {
        Path path = this;
        if (!IsAbsolute())
        {
            path = new Path(fs.GetWorkingDirectory(), this);
        }

        PathUri pathUri = path.ToUri();
        PathUri fsUri = fs.GetUri();

        string? scheme = pathUri.Scheme;
        string? authority = pathUri.Authority;

        if (scheme != null && (authority != null || fsUri.Authority == null))
        {
            return path;
        }

        scheme ??= fsUri.Scheme;
        authority ??= fsUri.Authority ?? "";

        return new Path(scheme + ":" + "//" + authority + pathUri.UriPath);
    }

    /// <summary>Does the path have a windows drive letter, e.g. /C:/foo (slashed) or C:/foo.</summary>
    private static bool HasWindowsDrive(string path, bool slashed)
    {
        int start = slashed ? 1 : 0;
        return path.Length >= start + 2
            && (!slashed || path[0] == '/')
            && path[start + 1] == ':'
            && char.IsAsciiLetter(path[start]);
    }

    /// <summary>Creates a path for the given local file, as a file URI like Java's
    /// <c>new Path(file.toURI())</c>.</summary>
    public static Path FromLocalFile(FileInfo file) =>
        new("file:" + AbsoluteUriPath(file.FullName));

    /// <summary>Renders a local filesystem path the way Java's <c>File.toURI().getPath()</c>
    /// does: forward slashes with a leading slash ("/C:/tmp/x" on Windows).</summary>
    internal static string AbsoluteUriPath(string localPath)
    {
        string path = localPath.Replace('\\', '/');
        return path.StartsWith('/') ? path : "/" + path;
    }

    // ------------------------------------------------------------------------
    //  Legacy Serialization
    // ------------------------------------------------------------------------

    /// <summary>
    /// Deserializes a path written with <see cref="SerializeToDataOutputView"/>.
    ///
    /// <para>PORT NOTE: the port serializes (scheme, authority, path) component-wise instead of
    /// Java's seven URI components.</para>
    /// </summary>
    public static Path? DeserializeFromDataInputView(IDataInputView input)
    {
        bool isNotNull = input.ReadBoolean();
        if (!isNotNull)
        {
            return null;
        }
        string? scheme = StringUtils.ReadNullableString(input);
        string? authority = StringUtils.ReadNullableString(input);
        string path = StringUtils.ReadString(input);
        return new Path(scheme, authority, path);
    }

    /// <summary>Serializes the path to the given output view.</summary>
    public static void SerializeToDataOutputView(Path? path, IDataOutputView output)
    {
        if (path is null)
        {
            output.WriteBoolean(false);
            return;
        }
        output.WriteBoolean(true);
        StringUtils.WriteNullableString(path._scheme, output);
        StringUtils.WriteNullableString(path._authority, output);
        StringUtils.WriteString(path._path, output);
    }
}
