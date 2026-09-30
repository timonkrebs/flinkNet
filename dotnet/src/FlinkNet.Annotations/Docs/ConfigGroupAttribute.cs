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

namespace FlinkNet.Annotations.Docs;

/// <summary>
/// A class that specifies a group of config options. The name of the group will be used as the
/// basis for the filename of the generated HTML file, as defined in the documentation generator.
///
/// <para>PORT NOTE: in Java this annotation can only appear inside <c>@ConfigGroups(groups = ...)</c>.
/// .NET attributes cannot take attribute instances as arguments, so the <c>ConfigGroups</c>
/// container is replaced by applying <c>[ConfigGroup]</c> repeatedly
/// (<see cref="AttributeUsageAttribute.AllowMultiple"/>).</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[Internal]
public sealed class ConfigGroupAttribute(string name, string keyPrefix) : Attribute
{
    public string Name { get; } = name;

    public string KeyPrefix { get; } = keyPrefix;
}
