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
/// A simple serializer interface for versioned serialization.
///
/// <para>The serializer has a version (returned by <see cref="IVersioned.Version"/>) which can
/// be attached to the serialized data. When the serializer evolves, the version can be used to
/// identify with which prior version the data was serialized.</para>
/// </summary>
/// <typeparam name="TElement">The data type serialized / deserialized by this serializer.</typeparam>
[PublicEvolving]
public interface ISimpleVersionedSerializer<TElement> : IVersioned
{
    /// <summary>
    /// Serializes the given object. The serialization is assumed to correspond to the current
    /// serializer version.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>The serialized data (bytes).</returns>
    byte[] Serialize(TElement obj);

    /// <summary>
    /// De-serializes the given data (bytes) which was serialized with the scheme of the
    /// indicated version.
    /// </summary>
    /// <param name="version">The version in which the data was serialized</param>
    /// <param name="serialized">The serialized data</param>
    /// <returns>The deserialized object</returns>
    TElement Deserialize(int version, byte[] serialized);
}
