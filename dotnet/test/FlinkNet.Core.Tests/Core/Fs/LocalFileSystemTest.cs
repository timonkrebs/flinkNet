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
using FlinkNet.Core.Fs;
using FlinkNet.Core.Fs.Local;
using Xunit;
using Path = FlinkNet.Core.Fs.Path;

namespace FlinkNet.Tests.Core.Fs;

/// <summary>
/// This class tests the functionality of the <see cref="LocalFileSystem"/> class in its components.
/// In particular, file/directory access, creation, deletion, read, write is tested.
/// </summary>
public class LocalFileSystemTest : IDisposable
{
    private readonly string _tmpDir =
        Directory.CreateTempSubdirectory("flinknet-localfs-test").FullName;

    public void Dispose()
    {
        if (Directory.Exists(_tmpDir))
        {
            Directory.Delete(_tmpDir, recursive: true);
        }
    }

    private Path TempPath(string name) => new(new Path(_tmpDir.Replace('\\', '/')), name);

    [Fact]
    public void TestLocalFilesystem()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();

        Path pathToTempFolder = TempPath("dir");
        Path pathToTmpFile = new(pathToTempFolder, "test_file");

        // the temporary directory does not yet exist
        Assert.False(lfs.Exists(pathToTempFolder));

        // create the directory
        Assert.True(lfs.Mkdirs(pathToTempFolder));
        Assert.True(lfs.Exists(pathToTempFolder));
        Assert.True(lfs.GetFileStatus(pathToTempFolder).IsDir);

        // write some data into a file
        byte[] testBytes = Encoding.UTF8.GetBytes("Hello FlinkNet local file system!");
        using (FSDataOutputStream outStream = lfs.Create(pathToTmpFile, FileSystem.WriteMode.NoOverwrite))
        {
            outStream.Write(testBytes, 0, testBytes.Length);
            Assert.Equal(testBytes.Length, outStream.GetPos());
            outStream.Sync();
        }

        // the file exists and has the right size
        IFileStatus status = lfs.GetFileStatus(pathToTmpFile);
        Assert.False(status.IsDir);
        Assert.Equal(testBytes.Length, status.Len);
        Assert.True(status.ModificationTime > 0);

        // read the data back, with a seek in between
        using (FSDataInputStream inStream = lfs.Open(pathToTmpFile))
        {
            byte[] buffer = new byte[testBytes.Length];
            inStream.Seek(6);
            Assert.Equal(6, inStream.GetPos());
            int read = inStream.Read(buffer, 0, buffer.Length - 6);
            Assert.Equal(testBytes.Length - 6, read);

            inStream.Seek(0);
            int total = 0;
            while (total < buffer.Length)
            {
                total += inStream.Read(buffer, total, buffer.Length - total);
            }
            Assert.Equal<byte[]>(testBytes, buffer);
        }

        // block locations point at localhost
        IBlockLocation[] locations = lfs.GetFileBlockLocations(status, 0, status.Len);
        Assert.Single(locations);
        Assert.Equal(new[] { "localhost" }, locations[0].GetHosts());

        // NO_OVERWRITE refuses to overwrite
        Assert.Throws<IOException>(() => lfs.Create(pathToTmpFile, FileSystem.WriteMode.NoOverwrite));

        // OVERWRITE truncates
        using (FSDataOutputStream outStream = lfs.Create(pathToTmpFile, FileSystem.WriteMode.Overwrite))
        {
            outStream.WriteByte(42);
        }
        Assert.Equal(1, lfs.GetFileStatus(pathToTmpFile).Len);

        // list the directory
        IFileStatus[] listed = lfs.ListStatus(pathToTempFolder);
        Assert.Single(listed);
        Assert.Equal("test_file", listed[0].Path.GetName());

        // rename the file
        Path renamed = new(pathToTempFolder, "renamed_file");
        Assert.True(lfs.Rename(pathToTmpFile, renamed));
        Assert.False(lfs.Exists(pathToTmpFile));
        Assert.True(lfs.Exists(renamed));

        // renaming a missing file fails
        Assert.False(lfs.Rename(pathToTmpFile, new Path(pathToTempFolder, "nope")));

        // non-recursive delete of a non-empty directory fails
        Assert.Throws<IOException>(() => lfs.Delete(pathToTempFolder, recursive: false));

        // recursive delete works
        Assert.True(lfs.Delete(pathToTempFolder, recursive: true));
        Assert.False(lfs.Exists(pathToTempFolder));
    }

    [Fact]
    public void TestCreateMakesParentDirectories()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();
        Path nested = TempPath("a/b/c/file.txt");

        using (FSDataOutputStream outStream = lfs.Create(nested, FileSystem.WriteMode.NoOverwrite))
        {
            outStream.WriteByte(1);
        }

        Assert.True(lfs.Exists(nested));
        Assert.True(lfs.GetFileStatus(TempPath("a/b/c")).IsDir);
    }

    [Fact]
    public void TestGetViaSchemeAndRegistry()
    {
        Assert.Same(FileSystem.GetLocalFileSystem(), FileSystem.Get(new Path("/some/path")));
        Assert.Same(FileSystem.GetLocalFileSystem(), FileSystem.Get(new Path("file:///some/path")));
        Assert.Same(
            FileSystem.GetLocalFileSystem(),
            new Path("file:///some/path").GetFileSystem());

        Assert.Throws<UnsupportedFileSystemSchemeException>(
            () => FileSystem.Get(new Path("nofs://host/some/path")));
    }

    /// <summary>A local URI carrying an authority ("file://tmp/out", missing a slash) must be
    /// rejected with a hint instead of silently operating on the wrong path.</summary>
    [Fact]
    public void TestGetRejectsLocalUriWithAuthority()
    {
        IOException e = Assert.Throws<IOException>(
            () => FileSystem.Get(new Path("file://tmp/out")));
        Assert.Contains("authority 'tmp'", e.Message);
        Assert.Contains("file:///tmp/out", e.Message);
    }

    [Fact]
    public void TestMkdirsReturnsFalseOnExistingFile()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();
        Path file = TempPath("plain_file");
        using (FSDataOutputStream outStream = lfs.Create(file, FileSystem.WriteMode.NoOverwrite))
        {
            outStream.WriteByte(7);
        }

        Assert.False(lfs.Mkdirs(file));
    }

    /// <summary>The working and home directories are file URIs with absolute paths, like
    /// Java's <c>new File(...).toURI()</c>.</summary>
    [Fact]
    public void TestWorkingAndHomeDirectoriesAreAbsoluteFileUris()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();

        Path workingDir = lfs.GetWorkingDirectory();
        Assert.Equal("file", workingDir.ToUri().Scheme);
        Assert.True(workingDir.IsAbsolute());
        Assert.Equal(Directory.GetCurrentDirectory().Replace('\\', '/').TrimStart('/'),
            workingDir.GetPath().TrimStart('/'));

        Path homeDir = lfs.GetHomeDirectory();
        Assert.Equal("file", homeDir.ToUri().Scheme);
        Assert.True(homeDir.IsAbsolute());
    }

    /// <summary>Java renames with REPLACE_EXISTING: committing a temp file over an existing
    /// destination must succeed.</summary>
    [Fact]
    public void TestRenameReplacesExistingTarget()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();

        Path src = TempPath("rename_src");
        Path dst = TempPath("rename_dst");
        File.WriteAllText(src.GetPath(), "new content");
        File.WriteAllText(dst.GetPath(), "old content");

        Assert.True(lfs.Rename(src, dst));
        Assert.False(lfs.Exists(src));
        Assert.Equal("new content", File.ReadAllText(dst.GetPath()));
    }

    /// <summary>Java creates the destination's parent directory before moving.</summary>
    [Fact]
    public void TestRenameCreatesMissingTargetParent()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();

        Path src = TempPath("rename_deep_src");
        Path dst = TempPath("missing_parent/rename_dst");
        File.WriteAllText(src.GetPath(), "content");

        Assert.True(lfs.Rename(src, dst));
        Assert.True(lfs.Exists(dst));
    }

    /// <summary>A non-empty destination directory is a regular move failure (Java's
    /// DirectoryNotEmptyException), reported as false.</summary>
    [Fact]
    public void TestRenameOntoNonEmptyDirectoryFails()
    {
        FileSystem lfs = FileSystem.GetLocalFileSystem();

        Path src = TempPath("rename_dir_src");
        Path dst = TempPath("occupied_dir");
        Assert.True(lfs.Mkdirs(src));
        Assert.True(lfs.Mkdirs(dst));
        File.WriteAllText(dst.GetPath() + "/occupant", "here");

        Assert.False(lfs.Rename(src, dst));
        Assert.True(lfs.Exists(src));
    }
}
