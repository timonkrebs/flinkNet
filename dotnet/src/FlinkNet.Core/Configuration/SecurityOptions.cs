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
/// The set of configuration options relating to security.
///
/// <para>PORT NOTE: only <see cref="AdditionalSensitiveKeys"/> is ported so far (needed by
/// <see cref="Configuration"/> for redaction); the SSL/Kerberos/ZooKeeper options follow with the
/// security subsystem increment.</para>
/// </summary>
[PublicEvolving]
public static class SecurityOptions
{
    /// <summary>
    /// Comma-separated list of additional configuration key substrings whose values should be
    /// redacted in logs and REST API responses.
    /// </summary>
    public static readonly ConfigOption<IList<string>> AdditionalSensitiveKeys =
        ConfigOptions.Key("security.redaction.additional-keys")
            .StringType()
            .AsList()
            .DefaultValues()
            .WithDescription(
                "Comma-separated list of additional configuration key substrings whose"
                    + " values should be redacted in logs and REST API responses."
                    + " Matching is case-insensitive and based on substring containment."
                    + " The built-in sensitive key patterns are immutable and cannot"
                    + " be overridden via this option.");
}
