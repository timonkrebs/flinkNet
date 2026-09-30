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
using FlinkNet.Api.Common.Serialization;
using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Api.Common.TypeUtils.Base;

namespace FlinkNet.Api.Common.TypeInfo;

/// <summary>
/// Type information for primitive types (int, long, double, byte, ...), and string.
///
/// <para>PORT NOTE: comparator classes are deferred to the sort/join runtime increment, and the
/// <c>IntegerTypeInfo</c>/<c>FractionalTypeInfo</c> intermediate classes are flattened into
/// <see cref="BasicTypeInfo{T}"/>. Date/Void/BigInteger/BigDecimal/Instant infos follow with
/// their serializers.</para>
/// </summary>
/// <typeparam name="T">The basic type.</typeparam>
[Public]
public class BasicTypeInfo<T> : TypeInformation<T>
{
    private readonly Type _clazz;

    private readonly Type[] _possibleCastTargetTypes;

    private readonly TypeSerializer<T> _serializer;

    internal BasicTypeInfo(Type clazz, Type[] possibleCastTargetTypes, TypeSerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(clazz);
        ArgumentNullException.ThrowIfNull(possibleCastTargetTypes);
        ArgumentNullException.ThrowIfNull(serializer);
        _clazz = clazz;
        _possibleCastTargetTypes = possibleCastTargetTypes;
        _serializer = serializer;
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns whether this type should be automatically casted to the target type in an
    /// arithmetic operation.
    /// </summary>
    public bool ShouldAutocastTo<TOther>(BasicTypeInfo<TOther> to)
    {
        foreach (Type possibleTo in _possibleCastTargetTypes)
        {
            if (possibleTo == to.TypeClass)
            {
                return true;
            }
        }
        return false;
    }

    // --------------------------------------------------------------------------------------------

    public override bool IsBasicType => true;

    public override bool IsTupleType => false;

    public override int Arity => 1;

    public override int TotalFields => 1;

    public override Type TypeClass => _clazz;

    public override bool IsKeyType => true;

    public override TypeSerializer<T> CreateSerializer(ISerializerConfig? config) => _serializer;

    // --------------------------------------------------------------------------------------------

    public override int GetHashCode() => HashCode.Combine(_clazz, _serializer);

    public override bool CanEqual(object obj) => obj is BasicTypeInfo<T>;

    public override bool Equals(object? obj) =>
        ReferenceEquals(obj, this)
            || (obj is BasicTypeInfo<T> other && other.CanEqual(this) && _clazz == other._clazz);

    public override string ToString() => _clazz.Name;
}

/// <summary>The static registry of <see cref="BasicTypeInfo{T}"/> instances.</summary>
[Public]
public static class BasicTypeInfo
{
    public static readonly BasicTypeInfo<string> StringTypeInfo =
        new(typeof(string), [], StringSerializer.Instance);

    public static readonly BasicTypeInfo<bool> BooleanTypeInfo =
        new(typeof(bool), [], BooleanSerializer.Instance);

    public static readonly BasicTypeInfo<byte> ByteTypeInfo =
        new(
            typeof(byte),
            [typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(char)],
            ByteSerializer.Instance);

    public static readonly BasicTypeInfo<short> ShortTypeInfo =
        new(
            typeof(short),
            [typeof(int), typeof(long), typeof(float), typeof(double), typeof(char)],
            ShortSerializer.Instance);

    public static readonly BasicTypeInfo<int> IntTypeInfo =
        new(
            typeof(int),
            [typeof(long), typeof(float), typeof(double), typeof(char)],
            IntSerializer.Instance);

    public static readonly BasicTypeInfo<long> LongTypeInfo =
        new(typeof(long), [typeof(float), typeof(double), typeof(char)], LongSerializer.Instance);

    public static readonly BasicTypeInfo<float> FloatTypeInfo =
        new(typeof(float), [typeof(double)], FloatSerializer.Instance);

    public static readonly BasicTypeInfo<double> DoubleTypeInfo =
        new(typeof(double), [], DoubleSerializer.Instance);

    public static readonly BasicTypeInfo<char> CharTypeInfo =
        new(typeof(char), [], CharSerializer.Instance);

    // --------------------------------------------------------------------------------------------

    private static readonly IReadOnlyDictionary<Type, object> Types =
        new Dictionary<Type, object>
        {
            { typeof(string), StringTypeInfo },
            { typeof(bool), BooleanTypeInfo },
            { typeof(byte), ByteTypeInfo },
            { typeof(short), ShortTypeInfo },
            { typeof(int), IntTypeInfo },
            { typeof(long), LongTypeInfo },
            { typeof(float), FloatTypeInfo },
            { typeof(double), DoubleTypeInfo },
            { typeof(char), CharTypeInfo },
        };

    /// <summary>
    /// Returns the basic type information for the given type, or null if the type is not a
    /// basic type.
    /// </summary>
    public static BasicTypeInfo<T>? GetInfoFor<T>() =>
        Types.TryGetValue(typeof(T), out object? info) ? (BasicTypeInfo<T>)info : null;
}
