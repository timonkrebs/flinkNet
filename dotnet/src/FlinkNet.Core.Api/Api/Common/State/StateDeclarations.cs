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
using FlinkNet.Api.Common.Functions;
using FlinkNet.Api.Common.TypeInfo;

namespace FlinkNet.Api.Common.State;

/// <summary>This is a helper class for declaring various states.</summary>
[Experimental]
public static class StateDeclarations
{
    /// <summary>
    /// Get the builder of <see cref="IAggregatingStateDeclaration{TIn, TAcc, TOut}"/>.
    /// </summary>
    public static AggregatingStateDeclarationBuilder<TIn, TOut, TAcc> AggregatingStateBuilder<TIn, TOut, TAcc>(
        string name,
        ITypeDescriptor<TAcc> aggTypeDescriptor,
        IAggregateFunction<TIn, TAcc, TOut> aggregateFunction)
    {
        return new AggregatingStateDeclarationBuilder<TIn, TOut, TAcc>(
            name, aggTypeDescriptor, aggregateFunction);
    }

    /// <summary>
    /// Get the builder of <see cref="IReducingStateDeclaration{T}"/>.
    /// </summary>
    public static ReducingStateDeclarationBuilder<T> ReducingStateBuilder<T>(
        string name, ITypeDescriptor<T> typeInformation, IReduceFunction<T> reduceFunction)
    {
        return new ReducingStateDeclarationBuilder<T>(name, typeInformation, reduceFunction);
    }

    /// <summary>
    /// Get the builder of <see cref="IMapStateDeclaration{TKey, TValue}"/>.
    /// </summary>
    public static MapStateDeclarationBuilder<TKey, TValue> MapStateBuilder<TKey, TValue>(
        string name,
        ITypeDescriptor<TKey> keyTypeInformation,
        ITypeDescriptor<TValue> valueTypeInformation)
    {
        return new MapStateDeclarationBuilder<TKey, TValue>(
            name, keyTypeInformation, valueTypeInformation);
    }

    /// <summary>
    /// Get the builder of <see cref="IListStateDeclaration{T}"/>.
    /// </summary>
    public static ListStateDeclarationBuilder<T> ListStateBuilder<T>(
        string name, ITypeDescriptor<T> elementTypeInformation)
    {
        return new ListStateDeclarationBuilder<T>(name, elementTypeInformation);
    }

    /// <summary>
    /// Get the builder of <see cref="IValueStateDeclaration{T}"/>.
    /// </summary>
    public static ValueStateDeclarationBuilder<T> ValueStateBuilder<T>(
        string name, ITypeDescriptor<T> valueType)
    {
        return new ValueStateDeclarationBuilder<T>(name, valueType);
    }

    /// <summary>
    /// Get the <see cref="IAggregatingStateDeclaration{TIn, TAcc, TOut}"/> of aggregating state.
    /// If you want to configure it more elaborately, use
    /// <see cref="AggregatingStateBuilder{TIn, TOut, TAcc}"/>.
    /// </summary>
    public static IAggregatingStateDeclaration<TIn, TAcc, TOut> AggregatingState<TIn, TAcc, TOut>(
        string name,
        ITypeDescriptor<TAcc> aggTypeDescriptor,
        IAggregateFunction<TIn, TAcc, TOut> aggregateFunction)
    {
        return new AggregatingStateDeclarationBuilder<TIn, TOut, TAcc>(
                name, aggTypeDescriptor, aggregateFunction)
            .Build();
    }

    /// <summary>
    /// Get the <see cref="IReducingStateDeclaration{T}"/> of list state. If you want to configure
    /// it more elaborately, use <see cref="ReducingStateBuilder{T}"/>.
    /// </summary>
    public static IReducingStateDeclaration<T> ReducingState<T>(
        string name, ITypeDescriptor<T> typeInformation, IReduceFunction<T> reduceFunction)
    {
        return new ReducingStateDeclarationBuilder<T>(name, typeInformation, reduceFunction)
            .Build();
    }

    /// <summary>
    /// Get the <see cref="IMapStateDeclaration{TKey, TValue}"/> of map state with
    /// <see cref="RedistributionMode.None"/>. If you want to configure it more elaborately, use
    /// <see cref="MapStateBuilder{TKey, TValue}"/>.
    /// </summary>
    public static IMapStateDeclaration<TKey, TValue> MapState<TKey, TValue>(
        string name,
        ITypeDescriptor<TKey> keyTypeInformation,
        ITypeDescriptor<TValue> valueTypeInformation)
    {
        return new MapStateDeclarationBuilder<TKey, TValue>(
                name, keyTypeInformation, valueTypeInformation)
            .Build();
    }

    /// <summary>
    /// Get the <see cref="IListStateDeclaration{T}"/> of list state with
    /// <see cref="RedistributionMode.None"/>. If you want to configure it more elaborately, use
    /// <see cref="ListStateBuilder{T}"/>.
    /// </summary>
    public static IListStateDeclaration<T> ListState<T>(
        string name, ITypeDescriptor<T> elementTypeInformation)
    {
        return new ListStateDeclarationBuilder<T>(name, elementTypeInformation).Build();
    }

    /// <summary>
    /// Get the <see cref="IValueStateDeclaration{T}"/> of value state. If you want to configure it
    /// more elaborately, use <see cref="ValueStateBuilder{T}"/>.
    /// </summary>
    public static IValueStateDeclaration<T> ValueState<T>(string name, ITypeDescriptor<T> valueType)
    {
        return new ValueStateDeclarationBuilder<T>(name, valueType).Build();
    }

    /// <summary>Builder for <see cref="IReducingStateDeclaration{T}"/>.</summary>
    [Experimental]
    public class ReducingStateDeclarationBuilder<T>
    {
        private readonly string _name;
        private readonly ITypeDescriptor<T> _typeInformation;

        private readonly IReduceFunction<T> _reduceFunction;

        public ReducingStateDeclarationBuilder(
            string name, ITypeDescriptor<T> typeInformation, IReduceFunction<T> reduceFunction)
        {
            _name = name;
            _typeInformation = typeInformation;
            _reduceFunction = reduceFunction;
        }

        internal IReducingStateDeclaration<T> Build()
        {
            return new ReducingStateDeclarationImpl(_name, _typeInformation, _reduceFunction);
        }

        private sealed class ReducingStateDeclarationImpl : IReducingStateDeclaration<T>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<T> _typeInformation;
            private readonly IReduceFunction<T> _reduceFunction;

            internal ReducingStateDeclarationImpl(
                string name, ITypeDescriptor<T> typeInformation, IReduceFunction<T> reduceFunction)
            {
                _name = name;
                _typeInformation = typeInformation;
                _reduceFunction = reduceFunction;
            }

            public ITypeDescriptor<T> TypeDescriptor => _typeInformation;

            public string Name => _name;

            public IReduceFunction<T> ReduceFunction => _reduceFunction;

            public RedistributionMode RedistributionMode => RedistributionMode.None;
        }
    }

    /// <summary>Builder for <see cref="IAggregatingStateDeclaration{TIn, TAcc, TOut}"/>.</summary>
    [Experimental]
    public class AggregatingStateDeclarationBuilder<TIn, TOut, TAcc>
    {
        private readonly string _name;

        private readonly ITypeDescriptor<TAcc> _stateTypeDescriptor;
        private readonly IAggregateFunction<TIn, TAcc, TOut> _aggregateFunction;

        public AggregatingStateDeclarationBuilder(
            string name,
            ITypeDescriptor<TAcc> stateTypeDescriptor,
            IAggregateFunction<TIn, TAcc, TOut> aggregateFunction)
        {
            _name = name;
            _stateTypeDescriptor = stateTypeDescriptor;
            _aggregateFunction = aggregateFunction;
        }

        internal IAggregatingStateDeclaration<TIn, TAcc, TOut> Build()
        {
            return new AggregatingStateDeclarationImpl(_name, _stateTypeDescriptor, _aggregateFunction);
        }

        private sealed class AggregatingStateDeclarationImpl
            : IAggregatingStateDeclaration<TIn, TAcc, TOut>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<TAcc> _stateTypeDescriptor;
            private readonly IAggregateFunction<TIn, TAcc, TOut> _aggregateFunction;

            internal AggregatingStateDeclarationImpl(
                string name,
                ITypeDescriptor<TAcc> stateTypeDescriptor,
                IAggregateFunction<TIn, TAcc, TOut> aggregateFunction)
            {
                _name = name;
                _stateTypeDescriptor = stateTypeDescriptor;
                _aggregateFunction = aggregateFunction;
            }

            public ITypeDescriptor<TAcc> TypeDescriptor => _stateTypeDescriptor;

            public IAggregateFunction<TIn, TAcc, TOut> AggregateFunction => _aggregateFunction;

            public string Name => _name;

            public RedistributionMode RedistributionMode => RedistributionMode.None;
        }
    }

    /// <summary>Builder for <see cref="IMapStateDeclaration{TKey, TValue}"/>.</summary>
    [Experimental]
    public class MapStateDeclarationBuilder<TKey, TValue>
    {
        private readonly string _name;
        private readonly ITypeDescriptor<TKey> _keyTypeInformation;
        private readonly ITypeDescriptor<TValue> _valueTypeInformation;

        private readonly RedistributionMode _redistributionMode;

        public MapStateDeclarationBuilder(
            string name,
            ITypeDescriptor<TKey> keyTypeInformation,
            ITypeDescriptor<TValue> valueTypeInformation)
            : this(name, keyTypeInformation, valueTypeInformation, RedistributionMode.None)
        {
        }

        public MapStateDeclarationBuilder(
            string name,
            ITypeDescriptor<TKey> keyTypeInformation,
            ITypeDescriptor<TValue> valueTypeInformation,
            RedistributionMode redistributionMode)
        {
            _name = name;
            _keyTypeInformation = keyTypeInformation;
            _valueTypeInformation = valueTypeInformation;
            _redistributionMode = redistributionMode;
        }

        public IBroadcastStateDeclaration<TKey, TValue> BuildBroadcast()
        {
            return new BroadcastStateDeclarationImpl(_name, _keyTypeInformation, _valueTypeInformation);
        }

        internal IMapStateDeclaration<TKey, TValue> Build()
        {
            return new MapStateDeclarationImpl(
                _name, _keyTypeInformation, _valueTypeInformation, _redistributionMode);
        }

        private sealed class BroadcastStateDeclarationImpl
            : IBroadcastStateDeclaration<TKey, TValue>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<TKey> _keyTypeInformation;
            private readonly ITypeDescriptor<TValue> _valueTypeInformation;

            internal BroadcastStateDeclarationImpl(
                string name,
                ITypeDescriptor<TKey> keyTypeInformation,
                ITypeDescriptor<TValue> valueTypeInformation)
            {
                _name = name;
                _keyTypeInformation = keyTypeInformation;
                _valueTypeInformation = valueTypeInformation;
            }

            public ITypeDescriptor<TKey> KeyTypeDescriptor => _keyTypeInformation;

            public ITypeDescriptor<TValue> ValueTypeDescriptor => _valueTypeInformation;

            public string Name => _name;

            public RedistributionMode RedistributionMode => RedistributionMode.Identical;
        }

        private sealed class MapStateDeclarationImpl : IMapStateDeclaration<TKey, TValue>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<TKey> _keyTypeInformation;
            private readonly ITypeDescriptor<TValue> _valueTypeInformation;
            private readonly RedistributionMode _redistributionMode;

            internal MapStateDeclarationImpl(
                string name,
                ITypeDescriptor<TKey> keyTypeInformation,
                ITypeDescriptor<TValue> valueTypeInformation,
                RedistributionMode redistributionMode)
            {
                _name = name;
                _keyTypeInformation = keyTypeInformation;
                _valueTypeInformation = valueTypeInformation;
                _redistributionMode = redistributionMode;
            }

            public ITypeDescriptor<TKey> KeyTypeDescriptor => _keyTypeInformation;

            public ITypeDescriptor<TValue> ValueTypeDescriptor => _valueTypeInformation;

            public string Name => _name;

            public RedistributionMode RedistributionMode => _redistributionMode;
        }
    }

    /// <summary>Builder for <see cref="IListStateDeclaration{T}"/>.</summary>
    [Experimental]
    public class ListStateDeclarationBuilder<T>
    {
        private readonly string _name;
        private readonly ITypeDescriptor<T> _elementTypeInformation;
        private RedistributionStrategy _redistributionStrategy = RedistributionStrategy.Split;
        private RedistributionMode _redistributionMode = RedistributionMode.None;

        public ListStateDeclarationBuilder(string name, ITypeDescriptor<T> elementTypeInformation)
        {
            _name = name;
            _elementTypeInformation = elementTypeInformation;
        }

        public ListStateDeclarationBuilder<T> RedistributeBy(RedistributionStrategy strategy)
        {
            _redistributionStrategy = strategy;
            _redistributionMode = RedistributionMode.Redistributable;
            return this;
        }

        public ListStateDeclarationBuilder<T> RedistributeWithMode(RedistributionMode mode)
        {
            _redistributionMode = mode;
            return this;
        }

        public IListStateDeclaration<T> Build()
        {
            return new ListStateDeclarationImpl(
                _name, _elementTypeInformation, _redistributionStrategy, _redistributionMode);
        }

        private sealed class ListStateDeclarationImpl : IListStateDeclaration<T>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<T> _elementTypeInformation;
            private readonly RedistributionStrategy _redistributionStrategy;
            private readonly RedistributionMode _redistributionMode;

            internal ListStateDeclarationImpl(
                string name,
                ITypeDescriptor<T> elementTypeInformation,
                RedistributionStrategy redistributionStrategy,
                RedistributionMode redistributionMode)
            {
                _name = name;
                _elementTypeInformation = elementTypeInformation;
                _redistributionStrategy = redistributionStrategy;
                _redistributionMode = redistributionMode;
            }

            public RedistributionStrategy RedistributionStrategy => _redistributionStrategy;

            public ITypeDescriptor<T> TypeDescriptor => _elementTypeInformation;

            public string Name => _name;

            public RedistributionMode RedistributionMode => _redistributionMode;
        }
    }

    /// <summary>Builder for <see cref="IValueStateDeclaration{T}"/>.</summary>
    [Experimental]
    public class ValueStateDeclarationBuilder<T>
    {
        private readonly string _name;
        private readonly ITypeDescriptor<T> _valueType;

        public ValueStateDeclarationBuilder(string name, ITypeDescriptor<T> valueType)
        {
            _name = name;
            _valueType = valueType;
        }

        internal IValueStateDeclaration<T> Build()
        {
            return new ValueStateDeclarationImpl(_name, _valueType);
        }

        private sealed class ValueStateDeclarationImpl : IValueStateDeclaration<T>
        {
            private readonly string _name;
            private readonly ITypeDescriptor<T> _valueType;

            internal ValueStateDeclarationImpl(string name, ITypeDescriptor<T> valueType)
            {
                _name = name;
                _valueType = valueType;
            }

            public ITypeDescriptor<T> TypeDescriptor => _valueType;

            public string Name => _name;

            public RedistributionMode RedistributionMode => RedistributionMode.None;
        }
    }
}
