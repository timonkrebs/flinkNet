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

using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Api.Common.TypeUtils.Base;

namespace FlinkNet.Tests.Api.Common.TypeUtils.Base;

/// <summary>A test for the <see cref="ListSerializer{T}"/>.</summary>
public class ListSerializerTest : SerializerTestBase<IList<long>>
{
    protected override TypeSerializer<IList<long>> CreateSerializer() =>
        new ListSerializer<long>(LongSerializer.Instance);

    protected override int ExpectedLength => -1;

    protected override IList<long>[] GetTestData()
    {
        var rnd = new Random(123654789);

        // empty lists
        IList<long> list1 = new List<long>();

        // single element lists
        IList<long> list2 = new List<long> { 12345L };

        // longer lists
        IList<long> list3 = Enumerable.Range(0, 100).Select(_ => rnd.NextInt64()).ToList();
        IList<long> list4 = Enumerable.Range(0, 30).Select(i => (long)i).ToList();

        return [list1, list2, list3, list4];
    }

    protected override bool DeepEquals(IList<long> expected, IList<long> actual) =>
        expected.SequenceEqual(actual);
}
