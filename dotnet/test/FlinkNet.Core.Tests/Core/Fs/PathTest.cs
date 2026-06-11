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

using FlinkNet.Core.Fs;
using FlinkNet.Core.Memory;
using Xunit;
using Path = FlinkNet.Core.Fs.Path;

namespace FlinkNet.Tests.Core.Fs;

/// <summary>Tests for the <see cref="Path"/> class.</summary>
public class PathTest
{
    [Fact]
    public void TestPathFromString()
    {
        var p = new Path("/my/path");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Null(p.ToUri().Scheme);

        p = new Path("/my/path/");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Null(p.ToUri().Scheme);

        p = new Path("/my//path/");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Null(p.ToUri().Scheme);

        p = new Path("/my//path//a///");
        Assert.Equal("/my/path/a", p.ToUri().UriPath);
        Assert.Null(p.ToUri().Scheme);

        p = new Path("\\my\\path\\\\a\\\\\\");
        Assert.Equal("/my/path/a", p.ToUri().UriPath);
        Assert.Null(p.ToUri().Scheme);

        p = new Path("hdfs:///my/path");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Equal("hdfs", p.ToUri().Scheme);

        p = new Path("hdfs:///my/path/");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Equal("hdfs", p.ToUri().Scheme);

        p = new Path("file:///my/path");
        Assert.Equal("/my/path", p.ToUri().UriPath);
        Assert.Equal("file", p.ToUri().Scheme);

        p = new Path("C:/my/windows/path");
        Assert.Equal("/C:/my/windows/path", p.ToUri().UriPath);

        p = new Path("file:/C:/my/windows/path");
        Assert.Equal("/C:/my/windows/path", p.ToUri().UriPath);

        Assert.ThrowsAny<Exception>(() => new Path((string)null!));
        Assert.ThrowsAny<Exception>(() => new Path(""));
    }

    [Fact]
    public void TestIsAbsolute()
    {
        // UNIX
        Assert.True(new Path("/my/abs/path").IsAbsolute());
        Assert.True(new Path("/").IsAbsolute());
        Assert.False(new Path("./my/rel/path").IsAbsolute());
        Assert.False(new Path("my/rel/path").IsAbsolute());

        // WINDOWS
        Assert.True(new Path("C:/my/abs/windows/path").IsAbsolute());
        Assert.True(new Path("y:/my/abs/windows/path").IsAbsolute());
        Assert.True(new Path("/y:/my/abs/windows/path").IsAbsolute());
        Assert.True(new Path("b:\\my\\abs\\windows\\path").IsAbsolute());
        Assert.True(new Path("/c:/my/dir").IsAbsolute());
        Assert.True(new Path("/C:/").IsAbsolute());
        Assert.False(new Path("C:").IsAbsolute());
        Assert.True(new Path("C:/").IsAbsolute());
        Assert.False(new Path("C:my\\relative\\path").IsAbsolute());
        Assert.True(new Path("\\my\\dir").IsAbsolute());
        Assert.True(new Path("\\").IsAbsolute());
        Assert.False(new Path(".\\my\\relative\\path").IsAbsolute());
        Assert.False(new Path("my\\relative\\path").IsAbsolute());
        Assert.True(new Path("\\\\myServer\\myDir").IsAbsolute());
    }

    [Fact]
    public void TestGetName()
    {
        Assert.Equal("path", new Path("/my/fancy/path").GetName());
        Assert.Equal("path", new Path("/my/fancy/path/").GetName());
        Assert.Equal("path", new Path("hdfs:///my/path").GetName());
        Assert.Equal("", new Path("/").GetName());
        Assert.Equal("path", new Path("C:/my/windows/path").GetName());
    }

    [Fact]
    public void TestGetParent()
    {
        Assert.Equal("/my/fancy", new Path("/my/fancy/path").GetParent()!.ToUri().UriPath);
        Assert.Equal("/my/other/fancy", new Path("/my/other/fancy/path/").GetParent()!.ToUri().UriPath);
        Assert.Equal("/my", new Path("hdfs:///my/path").GetParent()!.ToUri().UriPath);
        Assert.Equal("/", new Path("hdfs:///myPath/").GetParent()!.ToUri().UriPath);
        Assert.Null(new Path("/").GetParent());
        Assert.Equal("/C:/my/windows", new Path("C:/my/windows/path").GetParent()!.ToUri().UriPath);
    }

    [Fact]
    public void TestSuffix()
    {
        var p = new Path("/my/path");
        p = p.Suffix("_123");
        Assert.Equal("/my/path_123", p.ToUri().UriPath);

        p = new Path("/my/path/");
        p = p.Suffix("/abc");
        Assert.Equal("/my/path/abc", p.ToUri().UriPath);

        p = new Path("C:/my/windows/path");
        p = p.Suffix("/abc");
        Assert.Equal("/C:/my/windows/path/abc", p.ToUri().UriPath);
    }

    [Fact]
    public void TestDepth()
    {
        Assert.Equal(2, new Path("/my/path").Depth());
        Assert.Equal(3, new Path("/my/fancy/path/").Depth());
        Assert.Equal(
            12,
            new Path("/my/fancy/fancy/fancy/fancy/fancy/fancy/fancy/fancy/fancy/fancy/path").Depth());
        Assert.Equal(0, new Path("/").Depth());
        Assert.Equal(4, new Path("C:/my/windows/path").Depth());
    }

    [Fact]
    public void TestParsing()
    {
        const string scheme = "hdfs";
        const string authority = "localhost:8000";
        const string path = "/test/test";

        // correct usage
        // hdfs://localhost:8000/test/test
        PathUri u = new Path(scheme + "://" + authority + path).ToUri();
        Assert.Equal(scheme, u.Scheme);
        Assert.Equal(authority, u.Authority);
        Assert.Equal(path, u.UriPath);

        // hdfs:///test/test
        u = new Path(scheme + "://" + path).ToUri();
        Assert.Equal(scheme, u.Scheme);
        Assert.Null(u.Authority);
        Assert.Equal(path, u.UriPath);

        // hdfs:/test/test
        u = new Path(scheme + ":" + path).ToUri();
        Assert.Equal(scheme, u.Scheme);
        Assert.Null(u.Authority);
        Assert.Equal(path, u.UriPath);

        // incorrect usage
        // hdfs://test/test
        u = new Path(scheme + ":/" + path).ToUri();
        Assert.Equal(scheme, u.Scheme);
        Assert.Equal("test", u.Authority);
        Assert.Equal("/test", u.UriPath);

        // hdfs:////test/test
        u = new Path(scheme + ":///" + path).ToUri();
        Assert.Equal("hdfs", u.Scheme);
        Assert.Null(u.Authority);
        Assert.Equal(path, u.UriPath);
    }

    [Fact]
    public void TestToStringRoundTrips()
    {
        string[] paths =
        [
            "/my/path",
            "hdfs:///my/path",
            "hdfs://localhost:8000/test/test",
            "my/rel/path",
        ];
        foreach (string s in paths)
        {
            var p = new Path(s);
            Assert.Equal(p, new Path(p.ToString()));
        }

        // windows-drive relative form loses the internal leading slash in toString
        Assert.Equal("C:/my/windows/path", new Path("C:/my/windows/path").ToString());
    }

    [Fact]
    public void TestSerializationRoundTrip()
    {
        Path?[] paths =
        [
            new Path("/some/path"),
            new Path("hdfs://localhost:8000/test/test"),
            null,
        ];

        foreach (Path? path in paths)
        {
            var output = new DataOutputSerializer(32);
            Path.SerializeToDataOutputView(path, output);
            Path? restored =
                Path.DeserializeFromDataInputView(new DataInputDeserializer(output.GetCopyOfBuffer()));
            Assert.Equal(path, restored);
        }
    }
}
