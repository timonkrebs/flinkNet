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

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>Comparator for <c>char</c> values.</summary>
[Internal]
public sealed class CharComparator : BasicTypeComparator<char>
{
    public CharComparator(bool ascending)
        : base(ascending)
    {
    }

    protected override int CompareValues(char first, char second) => first.CompareTo(second);

    protected override int JavaHashCode(char value) => value;

    public override int CompareSerialized(IDataInputView firstSource, IDataInputView secondSource)
    {
        char v1 = firstSource.ReadChar();
        char v2 = secondSource.ReadChar();
        int comp = v1 < v2 ? -1 : (v1 == v2 ? 0 : 1);
        return AscendingComparison ? comp : -comp;
    }

    public override bool SupportsNormalizedKey => true;

    public override int NormalizeKeyLen => 2;

    public override bool IsNormalizedKeyPrefixOnly(int keyBytes) => keyBytes < 2;

    public override void PutNormalizedKey(char record, MemorySegment target, int offset, int numBytes) =>
        NormalizedKeyUtil.PutCharNormalizedKey(record, target, offset, numBytes);

    public override CharComparator Duplicate() => new(AscendingComparison);
}
