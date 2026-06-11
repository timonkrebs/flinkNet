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

namespace FlinkNet;

/// <summary>
/// Enumeration for Flink versions.
///
/// <para>It is used for API versioning, during SQL/Table API upgrades, and for migration tests.</para>
/// </summary>
[Public]
public enum FlinkVersion
{
    // NOTE: the version strings must not change,
    // as they are used to locate snapshot file paths.
    // The definition order (enum value) matters for performing version arithmetic.
    V1_3,
    V1_4,
    V1_5,
    V1_6,
    V1_7,
    V1_8,
    V1_9,
    V1_10,
    V1_11,
    V1_12,
    V1_13,
    V1_14,
    V1_15,
    V1_16,
    V1_17,
    V1_18,
    V1_19,
    V1_20,
    V2_0,
    V2_1,
    V2_2,
    V2_3,
    V2_4,
}

/// <summary>Operations on <see cref="FlinkVersion"/>, ported from the Java enum's members.</summary>
[Public]
public static class FlinkVersions
{
    private static readonly IReadOnlyDictionary<string, FlinkVersion> CodeMap =
        Enum.GetValues<FlinkVersion>().ToDictionary(VersionString);

    /// <summary>The version string, e.g. <c>"2.4"</c>. Matches the Java <c>toString()</c>.</summary>
    public static string VersionString(this FlinkVersion version) =>
        version.ToString()[1..].Replace('_', '.');

    public static bool IsNewerVersionThan(this FlinkVersion version, FlinkVersion otherVersion) =>
        version > otherVersion;

    /// <summary>Returns all versions within the defined range, inclusive both start and end.</summary>
    public static IReadOnlySet<FlinkVersion> RangeOf(FlinkVersion start, FlinkVersion end) =>
        Enum.GetValues<FlinkVersion>().Where(v => v >= start && v <= end).ToHashSet();

    public static FlinkVersion? ByCode(string code) =>
        CodeMap.TryGetValue(code, out var version) ? version : null;

    public static FlinkVersion ValueOf(int majorVersion, int minorVersion) =>
        Enum.Parse<FlinkVersion>($"V{majorVersion}_{minorVersion}");

    /// <summary>Returns the version for the current branch.</summary>
    public static FlinkVersion Current() => Enum.GetValues<FlinkVersion>()[^1];
}
