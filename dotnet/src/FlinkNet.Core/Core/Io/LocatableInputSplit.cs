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

/// <summary>
/// A locatable input split is an input split referring to input data which is located on one or
/// more hosts.
/// </summary>
[Public]
public class LocatableInputSplit : IInputSplit
{
    private static readonly string[] EmptyArr = [];

    /// <summary>The number of the split.</summary>
    private readonly int _splitNumber;

    /// <summary>The names of the hosts storing the data this input split refers to.</summary>
    private readonly string[] _hostnames;

    /// <summary>Creates a new locatable input split that refers to a multiple host as its data
    /// location.</summary>
    /// <param name="splitNumber">The number of the split</param>
    /// <param name="hostnames">The names of the hosts storing the data this input split refers to.</param>
    public LocatableInputSplit(int splitNumber, string[]? hostnames)
    {
        _splitNumber = splitNumber;
        _hostnames = hostnames ?? EmptyArr;
    }

    /// <summary>Creates a new locatable input split that refers to a single host as its data
    /// location.</summary>
    /// <param name="splitNumber">The number of the split.</param>
    /// <param name="hostname">The names of the host storing the data this input split refers to.</param>
    public LocatableInputSplit(int splitNumber, string? hostname)
    {
        _splitNumber = splitNumber;
        _hostnames = hostname == null ? EmptyArr : [hostname];
    }

    public int SplitNumber => _splitNumber;

    /// <summary>Returns the names of the hosts storing the data this input split refers to.</summary>
    public string[] GetHostnames() => _hostnames;

    public override int GetHashCode() => _splitNumber;

    public override bool Equals(object? obj) =>
        ReferenceEquals(obj, this)
            || (obj is LocatableInputSplit other
                && other._splitNumber == _splitNumber
                && other._hostnames.SequenceEqual(_hostnames));

    public override string ToString() =>
        "Locatable Split (" + _splitNumber + ") at [" + string.Join(", ", _hostnames) + "]";
}
