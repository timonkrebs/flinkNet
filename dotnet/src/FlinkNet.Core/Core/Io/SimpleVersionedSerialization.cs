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

using System.Buffers.Binary;
using FlinkNet.Annotations;
using FlinkNet.Core.Memory;

namespace FlinkNet.Core.Io;

/// <summary>
/// Utility methods for serialization of checkpoint metadata and such, based on
/// <see cref="ISimpleVersionedSerializer{TElement}"/>. The serialized data carries the
/// serializer version and payload length, so it can be deserialized by evolved serializers.
/// </summary>
[PublicEvolving]
public static class SimpleVersionedSerialization
{
    /// <summary>
    /// Serializes the version and datum into a stream: first the serializer's version (4 bytes,
    /// big endian), then the length of the serialized datum (4 bytes, big endian), then the
    /// serialized datum.
    /// </summary>
    /// <param name="serializer">The serializer to serialize the datum with.</param>
    /// <param name="datum">The datum to serialize.</param>
    /// <param name="output">The stream to serialize to.</param>
    public static void WriteVersionAndSerialize<T>(
        ISimpleVersionedSerializer<T> serializer, T datum, IDataOutputView output)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(output, "out");
        ArgumentNullException.ThrowIfNull(datum, "datum");

        byte[] data = serializer.Serialize(datum);

        output.WriteInt(serializer.Version);
        output.WriteInt(data.Length);
        output.Write(data);
    }

    /// <summary>
    /// Serializes the version and a list of data into a stream: version, list length, then the
    /// length-prefixed serialized elements.
    /// </summary>
    public static void WriteVersionAndSerializeList<T>(
        ISimpleVersionedSerializer<T> serializer, IList<T> data, IDataOutputView output)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(data, "data");
        ArgumentNullException.ThrowIfNull(output, "out");

        output.WriteInt(serializer.Version);
        output.WriteInt(data.Count);
        foreach (T datum in data)
        {
            byte[] serialized = serializer.Serialize(datum);
            output.WriteInt(serialized.Length);
            output.Write(serialized);
        }
    }

    /// <summary>
    /// Deserializes the version and datum from a stream, as written by
    /// <see cref="WriteVersionAndSerialize{T}(ISimpleVersionedSerializer{T}, T, IDataOutputView)"/>.
    /// </summary>
    /// <param name="serializer">The serializer to serialize the datum with.</param>
    /// <param name="input">The stream to deserialize from.</param>
    public static T ReadVersionAndDeSerialize<T>(
        ISimpleVersionedSerializer<T> serializer, IDataInputView input)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(input, "in");

        int version = input.ReadInt();
        int length = input.ReadInt();
        byte[] data = new byte[length];
        input.ReadFully(data);

        return serializer.Deserialize(version, data);
    }

    /// <summary>
    /// Deserializes the version and a list of data from a stream, as written by
    /// <see cref="WriteVersionAndSerializeList{T}"/>.
    /// </summary>
    public static List<T> ReadVersionAndDeserializeList<T>(
        ISimpleVersionedSerializer<T> serializer, IDataInputView input)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(input, "in");

        int serializerVersion = input.ReadInt();
        int listSize = input.ReadInt();
        var deserializedData = new List<T>(listSize);
        for (int i = 0; i < listSize; i++)
        {
            int length = input.ReadInt();
            byte[] data = new byte[length];
            input.ReadFully(data);
            deserializedData.Add(serializer.Deserialize(serializerVersion, data));
        }

        return deserializedData;
    }

    /// <summary>
    /// Serializes the version and datum into a byte array: the first four bytes are the
    /// serializer's version (big endian), the next four bytes the payload length (big endian),
    /// followed by the payload.
    /// </summary>
    /// <param name="serializer">The serializer to serialize the datum with.</param>
    /// <param name="datum">The datum to serialize.</param>
    /// <returns>A byte array containing the serialized version and serialized datum.</returns>
    public static byte[] WriteVersionAndSerialize<T>(
        ISimpleVersionedSerializer<T> serializer, T datum)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(datum, "datum");

        byte[] data = serializer.Serialize(datum);
        byte[] versionAndData = new byte[data.Length + 8];

        BinaryPrimitives.WriteInt32BigEndian(versionAndData.AsSpan(0), serializer.Version);
        BinaryPrimitives.WriteInt32BigEndian(versionAndData.AsSpan(4), data.Length);
        data.CopyTo(versionAndData, 8);

        return versionAndData;
    }

    /// <summary>
    /// Deserializes the version and datum from a byte array, as written by
    /// <see cref="WriteVersionAndSerialize{T}(ISimpleVersionedSerializer{T}, T)"/>.
    /// </summary>
    /// <param name="serializer">The serializer to deserialize the datum with.</param>
    /// <param name="bytes">The bytes to deserialize from.</param>
    /// <returns>The deserialized datum.</returns>
    public static T ReadVersionAndDeSerialize<T>(
        ISimpleVersionedSerializer<T> serializer, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(serializer, "serializer");
        ArgumentNullException.ThrowIfNull(bytes, "bytes");
        if (bytes.Length < 8)
        {
            throw new ArgumentException("byte array below minimum length (8 bytes)");
        }

        int version = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(0));
        int length = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(4));
        if (length != bytes.Length - 8)
        {
            throw new ArgumentException(
                "byte array length does not match the length written in the version header");
        }

        byte[] data = bytes.AsSpan(8).ToArray();
        return serializer.Deserialize(version, data);
    }
}
