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
}
