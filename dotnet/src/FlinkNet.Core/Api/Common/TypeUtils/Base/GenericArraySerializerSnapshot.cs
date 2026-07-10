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

using FlinkNet.Core.Memory;

namespace FlinkNet.Api.Common.TypeUtils.Base;

/// <summary>Point-in-time configuration of a <see cref="GenericArraySerializer{C}"/>.</summary>
/// <typeparam name="C">The component type.</typeparam>
public sealed class GenericArraySerializerSnapshot<C>
    : CompositeTypeSerializerSnapshot<C?[], GenericArraySerializer<C>>
{
    private const int SnapshotVersion = 1;

    private Type? _componentClass;

    /// <summary>Constructor to be used for read instantiation.</summary>
    public GenericArraySerializerSnapshot()
    {
    }

    /// <summary>Constructor to be used for writing the snapshot.</summary>
    public GenericArraySerializerSnapshot(GenericArraySerializer<C> genericArraySerializer)
        : base(genericArraySerializer)
    {
        _componentClass = genericArraySerializer.ComponentClass;
    }

    protected override int CurrentOuterSnapshotVersion => SnapshotVersion;

    protected override void WriteOuterSnapshot(IDataOutputView output) =>
        output.WriteUTF(_componentClass!.AssemblyQualifiedName!);

    protected override void ReadOuterSnapshot(int readOuterSnapshotVersion, IDataInputView input)
    {
        string componentClassName = input.ReadUTF();
        try
        {
            _componentClass = Type.GetType(componentClassName, throwOnError: true)!;
        }
        catch (Exception e)
        {
            throw new IOException(
                "Could not find the array component class '" + componentClassName + "'.", e);
        }
    }

    protected override OuterSchemaCompatibility ResolveOuterSchemaCompatibility(
        TypeSerializerSnapshot<C?[]> oldSerializerSnapshot)
    {
        if (oldSerializerSnapshot is not GenericArraySerializerSnapshot<C> oldSnapshot)
        {
            return OuterSchemaCompatibility.Incompatible;
        }
        return _componentClass == oldSnapshot._componentClass
            ? OuterSchemaCompatibility.CompatibleAsIs
            : OuterSchemaCompatibility.Incompatible;
    }

    protected override GenericArraySerializer<C> CreateOuterSerializerWithNestedSerializers(
        TypeSerializer[] nestedSerializers)
    {
        var componentSerializer = (TypeSerializer<C>)nestedSerializers[0];
        return new GenericArraySerializer<C>(componentSerializer);
    }

    protected override TypeSerializer[] GetNestedSerializers(
        GenericArraySerializer<C> outerSerializer) => [outerSerializer.ComponentSerializer];
}
