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
using FlinkNet.Core.Memory;
using FlinkNet.Util;

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>
/// Comparator for <c>float</c> values. <see cref="TypeComparator{T}.Compare"/> follows Java's
/// total order (<see cref="JavaPrimitives.Compare(float, float)"/>), while
/// <see cref="CompareSerialized"/> uses the IEEE operators as in Java, under which -0.0 and 0.0,
/// and NaN and any value, compare as equal. The type has no normalized key.
/// </summary>
[Internal]
public sealed class FloatComparator : BasicTypeComparator<float>
{
    public FloatComparator(bool ascending)
        : base(ascending)
    {
    }

    protected override int CompareValues(float first, float second) => JavaPrimitives.Compare(first, second);

    protected override int JavaHashCode(float value) => JavaPrimitives.HashCode(value);

    public override int CompareSerialized(IDataInputView firstSource, IDataInputView secondSource)
    {
        float l1 = firstSource.ReadFloat();
        float l2 = secondSource.ReadFloat();
        int comp = l1 < l2 ? -1 : (l1 > l2 ? 1 : 0);
        return AscendingComparison ? comp : -comp;
    }

    public override bool SupportsNormalizedKey => false;

    public override int NormalizeKeyLen => 0;

    public override bool IsNormalizedKeyPrefixOnly(int keyBytes) => true;

    public override void PutNormalizedKey(float record, MemorySegment target, int offset, int numBytes) =>
        throw new NotSupportedException();

    public override FloatComparator Duplicate() => new(AscendingComparison);
}
