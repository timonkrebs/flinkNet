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

/// <summary>
/// Write access to a configuration object. Allows storing values described with meta information
/// included in <see cref="ConfigOption{T}"/>.
/// </summary>
[PublicEvolving]
public interface IWritableConfig
{
    /// <summary>
    /// Stores a given value using the metadata included in the <see cref="ConfigOption{T}"/>. The
    /// value should be readable back through <see cref="IReadableConfig"/>.
    /// </summary>
    /// <typeparam name="T">type of the value to be stored</typeparam>
    /// <param name="option">metadata information</param>
    /// <param name="value">value to be stored</param>
    /// <returns>instance of this configuration for fluent API</returns>
    IWritableConfig Set<T>(ConfigOption<T> option, T value);
}
