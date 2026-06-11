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

namespace FlinkNet.Api.Common.TypeInfo;

// PORT NOTE: The Java implementation instantiates descriptor classes living in flink-core
// (org.apache.flink.api.common.typeinfo.descriptor.*) via reflection (TypeUtils.getInstance with
// Class.forName) to avoid a compile-time dependency cycle between flink-core-api and flink-core.
// flink-core is not ported yet, and .NET needs no such indirection, so the descriptors are
// implemented below as small internal classes that are instantiated directly. Their ToString and
// TypeClass behavior mirrors the Java descriptor implementations (which wrap BasicTypeInfo,
// ValueTypeInfo, ListTypeInfo and MapTypeInfo), with CLR type names taking the place of Java
// simple class names. The reflection helper org.apache.flink.api.common.typeinfo.utils.TypeUtils
// is therefore unnecessary and was not ported.

/// <summary>
/// Descriptor interface to create TypeInformation instances.
/// </summary>
[Experimental]
public static class TypeDescriptors
{
    public static ITypeDescriptor<T> Value<T>(ITypeDescriptor<T> typeDescriptor)
    {
        ArgumentNullException.ThrowIfNull(typeDescriptor);
        return new ValueTypeDescriptorImpl<T>(typeDescriptor);
    }

    public static ITypeDescriptor<IDictionary<TKey, TValue>> Map<TKey, TValue>(
        ITypeDescriptor<TKey> keyTypeDescriptor, ITypeDescriptor<TValue> valueTypeDescriptor)
    {
        ArgumentNullException.ThrowIfNull(keyTypeDescriptor);
        ArgumentNullException.ThrowIfNull(valueTypeDescriptor);
        return new MapTypeDescriptorImpl<TKey, TValue>(keyTypeDescriptor, valueTypeDescriptor);
    }

    public static ITypeDescriptor<IList<T>> List<T>(ITypeDescriptor<T> elementTypeDescriptor)
    {
        ArgumentNullException.ThrowIfNull(elementTypeDescriptor);
        return new ListTypeDescriptorImpl<T>(elementTypeDescriptor);
    }

    // BasicTypeInfo type descriptors
    public static readonly ITypeDescriptor<string> STRING = new BasicTypeDescriptorImpl<string>();
    public static readonly ITypeDescriptor<int> INT = new BasicTypeDescriptorImpl<int>();
    public static readonly ITypeDescriptor<bool> BOOLEAN = new BasicTypeDescriptorImpl<bool>();
    public static readonly ITypeDescriptor<long> LONG = new BasicTypeDescriptorImpl<long>();
    public static readonly ITypeDescriptor<byte> BYTE = new BasicTypeDescriptorImpl<byte>();
    public static readonly ITypeDescriptor<short> SHORT = new BasicTypeDescriptorImpl<short>();
    public static readonly ITypeDescriptor<double> DOUBLE = new BasicTypeDescriptorImpl<double>();
    public static readonly ITypeDescriptor<float> FLOAT = new BasicTypeDescriptorImpl<float>();
    public static readonly ITypeDescriptor<char> CHAR = new BasicTypeDescriptorImpl<char>();
}

/// <summary>
/// Implementation of <see cref="ITypeDescriptor{T}"/> for the basic types
/// (<c>string</c>, <c>int</c>, <c>bool</c>, ...).
/// </summary>
/// <typeparam name="T">type represented by this type descriptor.</typeparam>
[Internal]
internal sealed class BasicTypeDescriptorImpl<T> : ITypeDescriptor<T>
{
    public Type TypeClass => typeof(T);

    public override string ToString()
    {
        return "BasicTypeDescriptorImpl [basicTypeInfo=" + typeof(T).Name + "]";
    }
}

/// <summary>
/// Implementation of <see cref="ITypeDescriptor{T}"/> for value types.
/// </summary>
/// <typeparam name="T">type represented by this type descriptor.</typeparam>
// PORT NOTE: The Java counterpart constrains T to org.apache.flink.types.Value, which is not
// ported yet; the constraint is therefore omitted here.
[Internal]
internal sealed class ValueTypeDescriptorImpl<T> : ITypeDescriptor<T>
{
    private readonly Type _typeClass;

    public ValueTypeDescriptorImpl(ITypeDescriptor<T> typeDescriptor)
    {
        _typeClass = typeDescriptor.TypeClass;
    }

    public Type TypeClass => _typeClass;

    public override string ToString()
    {
        return "ValueTypeDescriptorImpl [valueTypeInfo=ValueType<" + _typeClass.Name + ">]";
    }
}

/// <summary>
/// Implementation of <see cref="ITypeDescriptor{T}"/> for map (dictionary) types.
/// </summary>
/// <typeparam name="TKey">type of the map keys.</typeparam>
/// <typeparam name="TValue">type of the map values.</typeparam>
[Internal]
internal sealed class MapTypeDescriptorImpl<TKey, TValue> : ITypeDescriptor<IDictionary<TKey, TValue>>
{
    private readonly Type _keyTypeClass;
    private readonly Type _valueTypeClass;

    public MapTypeDescriptorImpl(
        ITypeDescriptor<TKey> keyTypeDescriptor, ITypeDescriptor<TValue> valueTypeDescriptor)
    {
        _keyTypeClass = keyTypeDescriptor.TypeClass;
        _valueTypeClass = valueTypeDescriptor.TypeClass;
    }

    public Type TypeClass => typeof(IDictionary<TKey, TValue>);

    public override string ToString()
    {
        // The missing separator mirrors the Java MapTypeDescriptorImpl.toString().
        return "MapTypeDescriptorImpl"
            + "Map<" + _keyTypeClass.Name + ", " + _valueTypeClass.Name + ">";
    }
}

/// <summary>
/// Implementation of <see cref="ITypeDescriptor{T}"/> for list types.
/// </summary>
/// <typeparam name="T">type of the list elements.</typeparam>
[Internal]
internal sealed class ListTypeDescriptorImpl<T> : ITypeDescriptor<IList<T>>
{
    private readonly Type _elementTypeClass;

    public ListTypeDescriptorImpl(ITypeDescriptor<T> elementTypeDescriptor)
    {
        _elementTypeClass = elementTypeDescriptor.TypeClass;
    }

    public Type TypeClass => typeof(IList<T>);

    public override string ToString()
    {
        return "ListTypeDescriptorImpl [listTypeInfo=List<" + _elementTypeClass.Name + ">]";
    }
}
