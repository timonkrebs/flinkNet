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

/// <summary>A test for the <see cref="MapSerializer{TKey,TValue}"/>.</summary>
public class MapSerializerTest : SerializerTestBase<IDictionary<long, string>>
{
    protected override TypeSerializer<IDictionary<long, string>> CreateSerializer() =>
        new MapSerializer<long, string>(LongSerializer.Instance, StringSerializer.Instance);

    protected override int ExpectedLength => -1;

    protected override IDictionary<long, string>[] GetTestData()
    {
        var rnd = new Random(123654789);

        // empty maps
        IDictionary<long, string> map1 = new Dictionary<long, string>();

        // single element maps
        IDictionary<long, string> map2 = new Dictionary<long, string> { { 0L, "hello" } };

        // longer maps
        IDictionary<long, string> map3 = new Dictionary<long, string>();
        for (int i = 0; i < 200; i++)
        {
            map3[rnd.NextInt64()] = rnd.NextInt64().ToString();
        }

        // null-value maps (Flink's map format supports null values via a flag)
        IDictionary<long, string> map4 = new Dictionary<long, string>
        {
            { 0L, null! },
            { 5L, "value" },
            { -12L, null! },
        };

        return [map1, map2, map3, map4];
    }

    protected override bool DeepEquals(
        IDictionary<long, string> expected, IDictionary<long, string> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }
        foreach (KeyValuePair<long, string> entry in expected)
        {
            if (!actual.TryGetValue(entry.Key, out string? actualValue)
                || !Equals(entry.Value, actualValue))
            {
                return false;
            }
        }
        return true;
    }
}
