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

using FlinkNet.Configuration;
using Xunit;

namespace FlinkNet.Tests.Configuration;

/// <summary>Tests for <see cref="MemorySize.ToString"/>.</summary>
public class MemorySizePrettyPrintingTest
{
    public static TheoryData<long, string> Parameters() =>
        new()
        {
            { MemorySize.MemoryUnit.KiloBytes.Multiplier + 1, "1025 bytes" },
            { 100, "100 bytes" },
            { 1024, "1 kb" },
            {
                MemorySize.MemoryUnit.GigaBytes.Multiplier + 1,
                $"{MemorySize.MemoryUnit.GigaBytes.Multiplier + 1} bytes"
            },
            { 0, "0 bytes" },
        };

    [Theory]
    [MemberData(nameof(Parameters))]
    public void TestFormatting(long bytes, string expectedString)
    {
        Assert.Equal(expectedString, new MemorySize(bytes).ToString());
    }
}
