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

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>
/// Utilities for dealing with the <see cref="TypeSerializer{T}"/> and the
/// <see cref="TypeSerializerSnapshot{T}"/>.
/// </summary>
public static class TypeSerializerUtils
{
    /// <summary>Takes snapshots of the given serializers. In case where the snapshots are still
    /// extending the old <c>TypeSerializerConfigSnapshot</c> class, the snapshots are set up
    /// properly (PORT NOTE: the legacy config-snapshot bridge predates the port and is not
    /// carried over).</summary>
    public static TypeSerializerSnapshot[] Snapshot(params TypeSerializer[] originatingSerializers)
    {
        return Array.ConvertAll(originatingSerializers, s => s.UntypedSnapshotConfiguration());
    }
}
