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

namespace FlinkNet.Configuration;

/// <summary>Unmodifiable version of the Configuration class.</summary>
[Public]
public class UnmodifiableConfiguration : Configuration
{
    /// <summary>
    /// Creates a new UnmodifiableConfiguration, which holds a copy of the given configuration
    /// that cannot be altered.
    /// </summary>
    /// <param name="config">The configuration with the original contents.</param>
    public UnmodifiableConfiguration(Configuration config)
        : base(config)
    {
    }

    // --------------------------------------------------------------------------------------------
    //  All mutating methods must fail
    // --------------------------------------------------------------------------------------------

    public sealed override void AddAll(Configuration other) => Error();

    public sealed override void AddAll(Configuration other, string prefix) => Error();

    protected internal sealed override void SetValueInternal<T>(
        string key, T value, bool canBePrefixMap) =>
        Error();

    public override bool RemoveConfig<T>(ConfigOption<T> configOption)
    {
        Error();
        return false;
    }

    /// <summary>
    /// PORT NOTE: Java does not override <c>removeKey</c>, leaving a mutability hole; the port
    /// blocks it, consistent with the documented intent of the class.
    /// </summary>
    public override bool RemoveKey(string key)
    {
        Error();
        return false;
    }

    private static void Error() =>
        throw new NotSupportedException(
            "The configuration is unmodifiable; its contents cannot be changed.");
}
