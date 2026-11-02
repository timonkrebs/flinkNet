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

/// <summary>Tests for <see cref="StructuredOptionsSplitter"/>.</summary>
public class StructuredOptionsSplitterTest
{
    public static TheoryData<string, char, string?, string[]?> GetSpecs() =>
        new()
        {
            // Use single quotes for quoting
            { "'A;B';C", ';', null, ["A;B", "C"] },
            { "'A;B';'C'", ';', null, ["A;B", "C"] },
            { "A;B;C", ';', null, ["A", "B", "C"] },
            { "'AB''D;B';C", ';', null, ["AB'D;B", "C"] },
            { "A'BD;B';C", ';', null, ["A'BD", "B'", "C"] },
            { "'AB'D;B;C", ';', "Could not split string. Illegal quoting at position: 3", null },
            { "'A", ';', "Could not split string. Quoting was not closed properly.", null },
            { "C;'", ';', "Could not split string. Quoting was not closed properly.", null },

            // Use double quotes for quoting
            { "\"A;B\";C", ';', null, ["A;B", "C"] },
            { "\"A;B\";\"C\"", ';', null, ["A;B", "C"] },
            { "\"AB\"\"D;B\";C", ';', null, ["AB\"D;B", "C"] },
            { "A\"BD;B\";C", ';', null, ["A\"BD", "B\"", "C"] },
            { "\"AB\"D;B;C", ';', "Could not split string. Illegal quoting at position: 3", null },
            { "\"A", ';', "Could not split string. Quoting was not closed properly.", null },
            { "C;\"", ';', "Could not split string. Quoting was not closed properly.", null },

            // Mix different quoting
            { "'AB\"D';B;C", ';', null, ["AB\"D", "B", "C"] },
            { "'AB\"D;B';C", ';', null, ["AB\"D;B", "C"] },
            { "'AB\"''D;B';C", ';', null, ["AB\"'D;B", "C"] },
            { "\"AB'D\";B;C", ';', null, ["AB'D", "B", "C"] },
            { "\"AB'D;B\";C", ';', null, ["AB'D;B", "C"] },
            { "\"AB'\"\"D;B\";C", ';', null, ["AB'\"D;B", "C"] },

            // Use different delimiter
            { "'A,B',C", ',', null, ["A,B", "C"] },
            { "A,B,C", ',', null, ["A", "B", "C"] },

            // Whitespaces handling
            { "   'A;B'    ;   C   ", ';', null, ["A;B", "C"] },
            { "   A;B    ;   C   ", ';', null, ["A", "B", "C"] },
            { "'A;B'    ;C A", ';', null, ["A;B", "C A"] },
            { "' A    ;B'    ;'   C'", ';', null, [" A    ;B", "   C"] },
        };

    [Theory]
    [MemberData(nameof(GetSpecs))]
    public void TestParse(string str, char delimiter, string? expectedException, string[]? expectedSplits)
    {
        if (expectedException is not null)
        {
            var exception =
                Assert.Throws<ArgumentException>(
                    () => StructuredOptionsSplitter.SplitEscaped(str, delimiter));
            Assert.Contains(expectedException, exception.Message);
            return;
        }

        List<string> splits = StructuredOptionsSplitter.SplitEscaped(str, delimiter);

        Assert.Equal(expectedSplits, splits);
    }
}
