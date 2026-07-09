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

namespace FlinkNet.Core.Io;

/// <summary>A generic input split that has only a partition number.</summary>
[Public]
public class GenericInputSplit : IInputSplit
{
    /// <summary>The number of this split.</summary>
    private readonly int _partitionNumber;

    /// <summary>The total number of partitions.</summary>
    private readonly int _totalNumberOfPartitions;

    /// <summary>Creates a generic input split with the given split number.</summary>
    /// <param name="partitionNumber">The number of the split's partition.</param>
    /// <param name="totalNumberOfPartitions">The total number of the splits (partitions).</param>
    public GenericInputSplit(int partitionNumber, int totalNumberOfPartitions)
    {
        _partitionNumber = partitionNumber;
        _totalNumberOfPartitions = totalNumberOfPartitions;
    }

    public int SplitNumber => _partitionNumber;

    public int TotalNumberOfSplits => _totalNumberOfPartitions;

    public override int GetHashCode() => _partitionNumber ^ _totalNumberOfPartitions;

    public override bool Equals(object? obj) =>
        obj is GenericInputSplit other
            && _partitionNumber == other._partitionNumber
            && _totalNumberOfPartitions == other._totalNumberOfPartitions;

    public override string ToString() =>
        "GenericSplit (" + _partitionNumber + '/' + _totalNumberOfPartitions + ')';
}
