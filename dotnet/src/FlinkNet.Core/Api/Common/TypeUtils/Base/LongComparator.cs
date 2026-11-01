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

/// <summary>Comparator for <c>long</c> values.</summary>
[Internal]
public sealed class LongComparator : BasicTypeComparator<long>
{
    public LongComparator(bool ascending)
        : base(ascending)
    {
    }

    protected override int CompareValues(long first, long second) => first.CompareTo(second);

    protected override int JavaHashCode(long value) => JavaPrimitives.HashCode(value);

    public override int CompareSerialized(IDataInputView firstSource, IDataInputView secondSource)
    {
        long v1 = firstSource.ReadLong();
        long v2 = secondSource.ReadLong();
        int comp = v1 < v2 ? -1 : (v1 == v2 ? 0 : 1);
        return AscendingComparison ? comp : -comp;
    }

    public override bool SupportsNormalizedKey => true;

    public override int NormalizeKeyLen => 8;

    public override bool IsNormalizedKeyPrefixOnly(int keyBytes) => keyBytes < 8;

    public override void PutNormalizedKey(long record, MemorySegment target, int offset, int numBytes) =>
        NormalizedKeyUtil.PutLongNormalizedKey(record, target, offset, numBytes);

    public override LongComparator Duplicate() => new(AscendingComparison);
}
