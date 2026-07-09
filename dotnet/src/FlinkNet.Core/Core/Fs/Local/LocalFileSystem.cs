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

using FlinkNet.Annotations;

namespace FlinkNet.Core.Fs.Local;

/// <summary>
/// The class <c>LocalFileSystem</c> is an implementation of the <see cref="FileSystem"/>
/// interface for the local file system of the machine where the JVM runs (here: the CLR).
/// </summary>
[Internal]
public class LocalFileSystem : FileSystem
{
    /// <summary>The URI representing the local file system.</summary>
    private static readonly PathUri LocalUri = new("file", null, "/");

    /// <summary>The shared instance of the local file system.</summary>
    public static LocalFileSystem SharedInstance { get; } = new();

    /// <summary>Path pointing to the current working directory.</summary>
    private readonly Path _workingDir = new(new Uri(Directory.GetCurrentDirectory()).AbsolutePath);

    /// <summary>Path pointing to the current user home directory.</summary>
    private readonly Path _homeDir =
        new(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace('\\', '/'));

    public override IBlockLocation[] GetFileBlockLocations(IFileStatus file, long start, long len) =>
        [new LocalBlockLocation(file.Len)];

    public override IFileStatus GetFileStatus(Path f)
    {
        string filePath = PathToFilePath(f);
        if (Directory.Exists(filePath))
        {
            return new LocalFileStatus(new DirectoryInfo(filePath), this);
        }
        if (File.Exists(filePath))
        {
            return new LocalFileStatus(new FileInfo(filePath), this);
        }
        throw new FileNotFoundException(
            "File " + f + " does not exist or the user running Flink ('"
                + Environment.UserName
                + "') has insufficient permissions to access it.");
    }

    public override PathUri GetUri() => LocalUri;

    public override Path GetWorkingDirectory() => _workingDir;

    public override Path GetHomeDirectory() => _homeDir;

    public override FSDataInputStream Open(Path f)
    {
        string filePath = PathToFilePath(f);
        return new LocalDataInputStream(filePath);
    }

    public override bool Delete(Path f, bool recursive)
    {
        string filePath = PathToFilePath(f);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            if (Directory.Exists(filePath))
            {
                if (!recursive && Directory.EnumerateFileSystemEntries(filePath).Any())
                {
                    throw new IOException(
                        "Directory " + filePath + " is not empty and cannot be deleted "
                            + "non-recursively.");
                }
                Directory.Delete(filePath, recursive);
                return true;
            }
            return false;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            return false;
        }
    }

    public override IFileStatus[] ListStatus(Path f)
    {
        string filePath = PathToFilePath(f);
        if (File.Exists(filePath))
        {
            return [GetFileStatus(f)];
        }
        if (!Directory.Exists(filePath))
        {
            throw new FileNotFoundException("File " + f + " does not exist.");
        }

        return Directory
            .EnumerateFileSystemEntries(filePath)
            .Select(entry => GetFileStatus(new Path(f, System.IO.Path.GetFileName(entry))))
            .ToArray();
    }

    public override bool Mkdirs(Path f)
    {
        ArgumentNullException.ThrowIfNull(f);
        string filePath = PathToFilePath(f);
        if (File.Exists(filePath))
        {
            // a file with this name already exists
            return false;
        }
        Directory.CreateDirectory(filePath);
        return true;
    }

    public override FSDataOutputStream Create(Path f, WriteMode overwriteMode)
    {
        if (Exists(f) && overwriteMode == WriteMode.NoOverwrite)
        {
            throw new IOException(
                "File already exists: " + f + ". Use WriteMode.Overwrite to overwrite existing "
                    + "files and directories.");
        }

        Path? parent = f.GetParent();
        if (parent != null && !Mkdirs(parent) && !Exists(parent))
        {
            throw new IOException("Mkdirs failed to create " + parent);
        }

        string filePath = PathToFilePath(f);
        return new LocalDataOutputStream(filePath);
    }

    public override bool Rename(Path src, Path dst)
    {
        string srcPath = PathToFilePath(src);
        string dstPath = PathToFilePath(dst);

        // the move fails if the destination directory doesn't exist; Java ignores the
        // mkdirs() result, so creation failures surface through the move below
        string? dstParent = System.IO.Path.GetDirectoryName(dstPath);
        if (!string.IsNullOrEmpty(dstParent))
        {
            try
            {
                Directory.CreateDirectory(dstParent);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        // Java moves with REPLACE_EXISTING: an existing destination file or empty directory
        // is replaced, while the regular "move failed" conditions come back as false
        try
        {
            if (File.Exists(srcPath))
            {
                if (Directory.Exists(dstPath))
                {
                    Directory.Delete(dstPath, recursive: false);
                }
                File.Move(srcPath, dstPath, overwrite: true);
                return true;
            }
            if (Directory.Exists(srcPath))
            {
                if (File.Exists(dstPath))
                {
                    File.Delete(dstPath);
                }
                else if (Directory.Exists(dstPath))
                {
                    Directory.Delete(dstPath, recursive: false);
                }
                Directory.Move(srcPath, dstPath);
                return true;
            }
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    public override bool IsDistributedFS => false;

    /// <summary>Converts the given Flink path to a local file system path.</summary>
    internal static string PathToFilePath(Path path)
    {
        string uriPath = path.IsAbsolute() ? path.GetPath() : new Path(
            SharedInstance.GetWorkingDirectory(), path).GetPath();
        // strip the leading slash of Windows drive paths ("/C:/...")
        if (uriPath.Length >= 3 && uriPath[0] == '/' && uriPath[2] == ':')
        {
            uriPath = uriPath[1..];
        }
        return uriPath;
    }
}
