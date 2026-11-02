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

namespace FlinkNet.Configuration;

/// <summary>A key with FallbackKeys will fall back to the FallbackKeys if it itself is not configured.</summary>
public sealed class FallbackKey
{
    // -------------------------
    //  Factory methods
    // -------------------------

    internal static FallbackKey CreateFallbackKey(string key) => new(key, false);

    internal static FallbackKey CreateDeprecatedKey(string key) => new(key, true);

    // ------------------------------------------------------------------------

    private FallbackKey(string key, bool isDeprecated)
    {
        Key = key;
        IsDeprecated = isDeprecated;
    }

    public string Key { get; }

    public bool IsDeprecated { get; }

    // ------------------------------------------------------------------------

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        return obj is FallbackKey that && Key == that.Key && IsDeprecated == that.IsDeprecated;
    }

    public override int GetHashCode() => 31 * Key.GetHashCode() + (IsDeprecated ? 1 : 0);

    public override string ToString() => $"{{key={Key}, isDeprecated={(IsDeprecated ? "true" : "false")}}}";
}
