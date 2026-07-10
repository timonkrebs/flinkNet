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

/// <summary>Collection of attributes to modify the behavior of the documentation generators.</summary>
public static class Documentation
{
    /// <summary>Attribute used on config option fields to override the documented default.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Internal]
    public sealed class OverrideDefaultAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    /// <summary>
    /// Attribute used on config option fields to include them in specific sections. Sections are
    /// groups of options that are aggregated across option classes, with each group being placed
    /// into a dedicated file.
    ///
    /// <para>The <see cref="Position"/> argument controls the position in the generated table,
    /// with lower values being placed at the top. Fields with the same position are sorted
    /// alphabetically by key.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Internal]
    public sealed class SectionAttribute(params string[] value) : Attribute
    {
        /// <summary>The sections in the config docs where this option should be included.</summary>
        public string[] Value { get; } = value;

        /// <summary>The relative position of the option in its section.</summary>
        public int Position { get; init; } = int.MaxValue;
    }

    /// <summary>Constants for section names.</summary>
    public static class Sections
    {
        public const string CommonHostPort = "common_host_port";
        public const string CommonStateBackends = "common_state_backends";
        public const string CommonCheckpointing = "common_checkpointing";
        public const string CommonHighAvailability = "common_high_availability";
        public const string CommonHighAvailabilityZookeeper = "common_high_availability_zk";
        public const string CommonHighAvailabilityJobResultStore = "common_high_availability_jrs";
        public const string CommonHighAvailabilityApplicationResultStore = "common_high_availability_ars";
        public const string CommonMemory = "common_memory";
        public const string CommonMiscellaneous = "common_miscellaneous";

        public const string SecuritySsl = "security_ssl";
        public const string SecurityAuthKerberos = "security_auth_kerberos";
        public const string SecurityDelegationToken = "security_delegation_token";
        public const string SecurityAuthZookeeper = "security_auth_zk";

        public const string StateBackendRocksDb = "state_backend_rocksdb";
        public const string StateBackendForSt = "state_backend_forst";

        public const string StateLatencyTracking = "state_latency_tracking";
        public const string StateSizeTracking = "state_size_tracking";

        public const string StateChangelog = "state_changelog";

        public const string ExpertClassLoading = "expert_class_loading";
        public const string ExpertDebuggingAndTuning = "expert_debugging_and_tuning";
        public const string ExpertScheduling = "expert_scheduling";
        public const string ExpertFaultTolerance = "expert_fault_tolerance";
        public const string ExpertCheckpointing = "expert_checkpointing";
        public const string ExpertRest = "expert_rest";
        public const string ExpertHighAvailability = "expert_high_availability";
        public const string ExpertZookeeperHighAvailability = "expert_high_availability_zk";
        public const string ExpertKubernetesHighAvailability = "expert_high_availability_k8s";
        public const string ExpertSecuritySsl = "expert_security_ssl";
        public const string ExpertRocksDb = "expert_rocksdb";
        public const string ExpertForSt = "expert_forst";
        public const string ExpertCluster = "expert_cluster";
        public const string ExpertJobManager = "expert_jobmanager";

        public const string AllJobManager = "all_jobmanager";
        public const string AllTaskManager = "all_taskmanager";
        public const string AllTaskManagerNetwork = "all_taskmanager_network";

        public const string DeprecatedFileSinks = "deprecated_file_sinks";

        public const string MetricReporters = "metric_reporters";

        public const string TraceReporters = "trace_reporters";

        public const string EventReporters = "event_reporters";

        public const string CheckpointFileMerging = "checkpoint_file_merging";

        public const string ModelOpenAiCommon = "model_openai_common";

        public const string ModelOpenAiChat = "model_openai_chat";

        public const string ModelOpenAiEmbedding = "model_openai_embedding";

        public const string ModelTritonCommon = "model_triton_common";

        public const string ModelTritonAdvanced = "model_triton_advanced";
    }

    /// <summary>
    /// Attribute used on table config options for adding meta data labels.
    ///
    /// <para>The <see cref="ExecMode"/> argument indicates the execution mode the config works for
    /// (batch, streaming or both).</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Internal]
    public sealed class TableOptionAttribute(Documentation.ExecMode execMode) : Attribute
    {
        public Documentation.ExecMode ExecMode { get; } = execMode;
    }

    /// <summary>The execution mode the config works for.</summary>
    public enum ExecMode
    {
        Batch,
        Streaming,
        BatchStreaming,
    }

    /// <summary>Returns the display name of an <see cref="ExecMode"/>, matching the Java <c>toString()</c>.</summary>
    public static string GetDisplayName(this ExecMode mode) =>
        mode switch
        {
            ExecMode.Batch => "Batch",
            ExecMode.Streaming => "Streaming",
            ExecMode.BatchStreaming => "Batch and Streaming",
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };

    /// <summary>
    /// Attribute used on config option fields or options classes to mark them as a suffix-option;
    /// i.e., a config option where the key is only a suffix, with the prefix being dynamically
    /// provided at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    [Internal]
    public sealed class SuffixOptionAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    /// <summary>
    /// Attribute used on config option fields or REST API message headers to exclude them from
    /// documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    [Internal]
    public sealed class ExcludeFromDocumentationAttribute(string value = "") : Attribute
    {
        /// <summary>The optional reason why it is excluded from documentation.</summary>
        public string Value { get; } = value;
    }
}
