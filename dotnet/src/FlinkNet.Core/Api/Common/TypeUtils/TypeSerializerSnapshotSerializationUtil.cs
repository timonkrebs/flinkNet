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

using FlinkNet.Core.Io;
using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils;

/// <summary>Utility methods for serialization of <see cref="TypeSerializerSnapshot"/>.</summary>
public static class TypeSerializerSnapshotSerializationUtil
{
    /// <summary>
    /// Writes a <see cref="TypeSerializerSnapshot"/> to the provided data output view.
    ///
    /// <para>It is written with a format that can be later read again using
    /// <see cref="ReadSerializerSnapshot"/>.</para>
    /// </summary>
    /// <param name="output">the data output view</param>
    /// <param name="serializerSnapshot">the serializer configuration snapshot to write</param>
    public static void WriteSerializerSnapshot(
        IDataOutputView output, TypeSerializerSnapshot serializerSnapshot)
    {
        new TypeSerializerSnapshotSerializationProxy(serializerSnapshot).Write(output);
    }

    /// <summary>
    /// Reads from a data input view a <see cref="TypeSerializerSnapshot"/> that was previously
    /// written using <see cref="WriteSerializerSnapshot"/>.
    /// </summary>
    /// <param name="input">the data input view</param>
    public static TypeSerializerSnapshot ReadSerializerSnapshot(IDataInputView input)
    {
        var proxy = new TypeSerializerSnapshotSerializationProxy();
        proxy.Read(input);
        return proxy.SerializerSnapshot!;
    }

    /// <summary>
    /// Reads a snapshot class name from the stream and instantiates it via its nullary
    /// constructor.
    ///
    /// <para>PORT NOTE: replaces Java's <c>InstantiationUtil.resolveClassByName</c> +
    /// <c>instantiate</c>; the stream carries a CLR assembly-qualified type name.</para>
    /// </summary>
    public static TypeSerializerSnapshot ReadAndInstantiateSnapshotClass(IDataInputView input)
    {
        string className = input.ReadUTF();

        Type type;
        try
        {
            type = Type.GetType(className, throwOnError: true)!;
        }
        catch (Exception e)
        {
            throw new IOException(
                "Could not find class '" + className + "' in the current runtime.", e);
        }

        if (!typeof(TypeSerializerSnapshot).IsAssignableFrom(type))
        {
            throw new IOException(
                "The class " + className + " is not a subclass of "
                    + typeof(TypeSerializerSnapshot).FullName + ".");
        }

        try
        {
            return (TypeSerializerSnapshot)Activator.CreateInstance(type)!;
        }
        catch (Exception e)
        {
            throw new IOException(
                "Could not instantiate the snapshot class '" + className
                    + "'. Snapshot classes must have a public nullary constructor.", e);
        }
    }

    /// <summary>
    /// Utility serialization proxy for a <see cref="TypeSerializerSnapshot"/>.
    ///
    /// <para>Binary format layout of a written serializer snapshot is as follows:</para>
    /// <list type="number">
    /// <item><description>Format version of this util.</description></item>
    /// <item><description>Name of the TypeSerializerSnapshot class.</description></item>
    /// <item><description>The version of the TypeSerializerSnapshot's binary format.</description></item>
    /// <item><description>The actual serializer snapshot data.</description></item>
    /// </list>
    /// </summary>
    private sealed class TypeSerializerSnapshotSerializationProxy : VersionedIOReadableWritable
    {
        private const int ProxyVersion = 2;

        /// <summary>Constructor for reading serializers.</summary>
        public TypeSerializerSnapshotSerializationProxy()
        {
        }

        /// <summary>Constructor for writing out serializers.</summary>
        public TypeSerializerSnapshotSerializationProxy(TypeSerializerSnapshot serializerSnapshot)
        {
            SerializerSnapshot = serializerSnapshot;
        }

        public TypeSerializerSnapshot? SerializerSnapshot { get; private set; }

        public override int Version => ProxyVersion;

        public override void Write(IDataOutputView output)
        {
            // write the format version of this util's format
            base.Write(output);

            TypeSerializerSnapshot.WriteVersionedSnapshot(output, SerializerSnapshot!);
        }

        public override void Read(IDataInputView input)
        {
            // read version
            base.Read(input);
            int version = GetReadVersion();

            SerializerSnapshot = version switch
            {
                2 => TypeSerializerSnapshot.ReadVersionedSnapshot(input),
                _ => throw new IOException(
                    "Unrecognized version for TypeSerializerSnapshot format: " + version),
            };
        }

        public override string? GetAdditionalDetailsForIncompatibleVersion(int readVersion)
        {
            if (readVersion == 1)
            {
                return "As of Flink 1.17 TypeSerializerConfigSnapshot is no longer supported. "
                    + "In order to upgrade Flink to 1.17+ you need to first migrate your "
                    + "serializers to use TypeSerializerSnapshot instead. Please first take a "
                    + "savepoint in Flink 1.16 without using TypeSerializerConfigSnapshot. "
                    + "After that you can use this savepoint to upgrade to Flink 1.17.";
            }
            return null;
        }
    }
}
