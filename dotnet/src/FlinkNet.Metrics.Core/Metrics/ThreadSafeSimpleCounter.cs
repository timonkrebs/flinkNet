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

namespace FlinkNet.Metrics;

/// <summary>
/// A simple low-overhead <see cref="ICounter"/> that is thread-safe (Java's LongAdder maps to
/// <see cref="Interlocked"/> operations).
/// </summary>
[Internal]
public class ThreadSafeSimpleCounter : ICounter
{
    /// <summary>The current count.</summary>
    private long _count;

    public void Inc() => Interlocked.Increment(ref _count);

    public void Inc(long n) => Interlocked.Add(ref _count, n);

    public void Dec() => Interlocked.Decrement(ref _count);

    public void Dec(long n) => Interlocked.Add(ref _count, -n);

    public long Count => Interlocked.Read(ref _count);
}
