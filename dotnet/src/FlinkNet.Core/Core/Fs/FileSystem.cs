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
using FlinkNet.Core.Fs.Local;

namespace FlinkNet.Core.Fs;

/// <summary>
/// Abstract base class of all file systems used by Flink. This class may be extended to implement
/// distributed file systems, or local file systems.
///
/// <para>The data persistence contract, consistency expectations, and thread-safety notes of the
/// Java class apply unchanged: streams are not thread-safe, file system instances may be shared
/// between threads, and data is only visible/durable after the writing stream is closed.</para>
///
/// <para>PORT NOTE: Java loads file systems via plugins/service loaders and configures them from
/// the Flink configuration; the port currently uses a simple scheme registry
/// (<see cref="RegisterFileSystem"/>) with the local file system built in. The safety-net
/// machinery is deferred to the runtime increment.</para>
/// </summary>
[Public]
public abstract class FileSystem
{
    /// <summary>
    /// The possible write modes. The write mode decides what happens if a file should be created,
    /// but already exists.
    /// </summary>
    public enum WriteMode
    {
        /// <summary>
        /// Creates the target file only if no file exists at that path already. Does not
        /// overwrite existing files and directories.
        /// </summary>
        NoOverwrite,

        /// <summary>
        /// Creates a new target file regardless of any existing files or directories. Existing
        /// files and directories will be deleted (recursively) automatically before creating the
        /// new file.
        /// </summary>
        Overwrite,
    }

    // ------------------------------------------------------------------------
    //  File System Implementation Registry
    // ------------------------------------------------------------------------

    private static readonly Dictionary<string, FileSystem> Registry = new()
    {
        { "file", LocalFileSystem.SharedInstance },
    };

    private static readonly object RegistryLock = new();

    /// <summary>Returns a reference to the FileSystem instance for accessing the local file system.</summary>
    public static FileSystem GetLocalFileSystem() => LocalFileSystem.SharedInstance;

    /// <summary>
    /// Returns a reference to the FileSystem instance for accessing the file system identified
    /// by the given path.
    /// </summary>
    /// <param name="path">The path of the file system to access.</param>
    /// <returns>a reference to the FileSystem instance for accessing the file system.</returns>
    /// <exception cref="IOException">thrown if no file system can be identified for the scheme.</exception>
    public static FileSystem Get(Path path)
    {
        string? scheme = path.ToUri().Scheme;
        if (scheme is null or "file")
        {
            return LocalFileSystem.SharedInstance;
        }
        lock (RegistryLock)
        {
            if (Registry.TryGetValue(scheme, out FileSystem? fs))
            {
                return fs;
            }
        }
        throw new UnsupportedFileSystemSchemeException(
            "Could not find a file system implementation for scheme '"
                + scheme
                + "'. File system schemes are registered via RegisterFileSystem.");
    }

    /// <summary>Registers a file system for the given scheme (PORT NOTE: stand-in for Java's
    /// plugin/service-loader mechanism).</summary>
    public static void RegisterFileSystem(string scheme, FileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(fileSystem);
        lock (RegistryLock)
        {
            Registry[scheme] = fileSystem;
        }
    }

    // ------------------------------------------------------------------------
    //  File System Methods
    // ------------------------------------------------------------------------

    /// <summary>Returns the path of the file system's current working directory.</summary>
    public abstract Path GetWorkingDirectory();

    /// <summary>Returns the path of the user's home directory in this file system.</summary>
    public abstract Path GetHomeDirectory();

    /// <summary>Returns the URI components whose scheme and authority identify this file system.</summary>
    public abstract PathUri GetUri();

    /// <summary>
    /// Return a file status object that represents the path.
    /// </summary>
    /// <param name="f">The path we want information from</param>
    /// <returns>a FileStatus object</returns>
    /// <exception cref="FileNotFoundException">when the path does not exist</exception>
    public abstract IFileStatus GetFileStatus(Path f);

    /// <summary>
    /// Return an array containing hostnames, offset and size of portions of the given file.
    /// </summary>
    public abstract IBlockLocation[] GetFileBlockLocations(IFileStatus file, long start, long len);

    /// <summary>
    /// Opens an FSDataInputStream at the indicated Path.
    /// </summary>
    /// <param name="f">the file to open</param>
    public abstract FSDataInputStream Open(Path f);

    /// <summary>
    /// List the statuses of the files/directories in the given path if the path is a directory.
    /// </summary>
    /// <param name="f">given path</param>
    /// <returns>the statuses of the files/directories in the given path</returns>
    public abstract IFileStatus[] ListStatus(Path f);

    /// <summary>
    /// Check if exists.
    /// </summary>
    /// <param name="f">source file</param>
    public virtual bool Exists(Path f)
    {
        try
        {
            return GetFileStatus(f) != null;
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

    /// <summary>
    /// Delete a file.
    /// </summary>
    /// <param name="f">the path to delete</param>
    /// <param name="recursive">if path is a directory and set to true, the directory is deleted
    /// else throws an exception. In case of a file the recursive can be set to either true or
    /// false</param>
    /// <returns>true if delete is successful, false otherwise</returns>
    public abstract bool Delete(Path f, bool recursive);

    /// <summary>
    /// Make the given file and all non-existent parents into directories. Has the semantics of
    /// Unix 'mkdir -p'. Existence of the directory hierarchy is not an error.
    /// </summary>
    /// <param name="f">the directory/directories to be created</param>
    /// <returns>true if at least one new directory has been created, false otherwise</returns>
    public abstract bool Mkdirs(Path f);

    /// <summary>
    /// Opens an FSDataOutputStream to a new file at the given path.
    ///
    /// <para>If the file already exists, the behavior depends on the given WriteMode. If the mode
    /// is set to <see cref="WriteMode.NoOverwrite"/>, then this method fails with an exception.</para>
    /// </summary>
    /// <param name="f">The file path to write to</param>
    /// <param name="overwriteMode">The action to take if a file or directory already exists at
    /// the given path.</param>
    /// <returns>The stream to the new file at the target path.</returns>
    public abstract FSDataOutputStream Create(Path f, WriteMode overwriteMode);

    /// <summary>
    /// Renames the file/directory src to dst.
    /// </summary>
    /// <param name="src">the file/directory to rename</param>
    /// <param name="dst">the new name of the file/directory</param>
    /// <returns>true if the renaming was successful, false otherwise</returns>
    public abstract bool Rename(Path src, Path dst);

    /// <summary>
    /// Returns true if this is a distributed file system. A distributed file system here means
    /// that the file system is shared among all Flink processes that participate in a cluster or
    /// job and that all these processes can see the same files.
    /// </summary>
    public abstract bool IsDistributedFS { get; }
}

/// <summary>
/// An exception to indicate that a specific file system scheme is not supported.
/// </summary>
[Public]
public class UnsupportedFileSystemSchemeException : IOException
{
    /// <summary>Creates a new exception with the given message.</summary>
    public UnsupportedFileSystemSchemeException(string message)
        : base(message)
    {
    }

    /// <summary>Creates a new exception with the given message and cause.</summary>
    public UnsupportedFileSystemSchemeException(string message, Exception cause)
        : base(message, cause)
    {
    }
}
