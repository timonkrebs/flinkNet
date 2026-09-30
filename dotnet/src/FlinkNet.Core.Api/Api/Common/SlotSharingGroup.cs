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
using FlinkNet.Configuration;

namespace FlinkNet.Api.Common;

/// <summary>
/// Describe the name and the different resource components of a slot sharing group.
///
/// <para>Two SlotSharingGroup classes currently exist in flink-core and flink-core-api, because
/// the one in flink-core exposes components marked as internal, which we want to avoid in
/// DataStream-V2 API. Once the V1 API is removed, we will remove the legacy
/// SlotSharingGroup.</para>
/// </summary>
[Experimental]
public class SlotSharingGroup
{
    private SlotSharingGroup(
        string name,
        double? cpuCores,
        MemorySize? taskHeapMemory,
        MemorySize? taskOffHeapMemory,
        MemorySize? managedMemory,
        IDictionary<string, double> extendedResources)
    {
        Name = name;
        CpuCores = cpuCores;
        TaskHeapMemory = taskHeapMemory;
        TaskOffHeapMemory = taskOffHeapMemory;
        ManagedMemory = managedMemory;
        ExternalResources = new Dictionary<string, double>(extendedResources).AsReadOnly();
    }

    private SlotSharingGroup(string name)
        : this(name, null, null, null, null, new Dictionary<string, double>())
    {
    }

    public string Name { get; }

    /// <summary>How many cpu cores are needed. Can be null only if it is unknown.</summary>
    public double? CpuCores { get; }

    /// <summary>How much task heap memory is needed.</summary>
    public MemorySize? TaskHeapMemory { get; }

    /// <summary>How much task off-heap memory is needed.</summary>
    public MemorySize? TaskOffHeapMemory { get; }

    /// <summary>How much managed memory is needed.</summary>
    public MemorySize? ManagedMemory { get; }

    /// <summary>
    /// A extensible field for user specified resources from <see cref="SlotSharingGroup"/>.
    /// </summary>
    public IReadOnlyDictionary<string, double> ExternalResources { get; }

    public static Builder NewBuilder(string name)
    {
        return new Builder(name);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj != null && obj.GetType() == typeof(SlotSharingGroup))
        {
            SlotSharingGroup that = (SlotSharingGroup)obj;
            return Nullable.Equals(CpuCores, that.CpuCores)
                && Equals(TaskHeapMemory, that.TaskHeapMemory)
                && Equals(TaskOffHeapMemory, that.TaskOffHeapMemory)
                && Equals(ManagedMemory, that.ManagedMemory)
                && ExternalResourcesEqual(ExternalResources, that.ExternalResources);
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Mirrors java.util.Map.hashCode(): an order-independent, content-based hash of the
        // external resources (C# dictionaries hash by reference).
        int externalResourcesHash = 0;
        foreach (KeyValuePair<string, double> resource in ExternalResources)
        {
            externalResourcesHash += HashCode.Combine(resource.Key, resource.Value);
        }
        return HashCode.Combine(
            CpuCores, TaskHeapMemory, TaskOffHeapMemory, ManagedMemory, externalResourcesHash);
    }

    private static bool ExternalResourcesEqual(
        IReadOnlyDictionary<string, double> left, IReadOnlyDictionary<string, double> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }
        foreach (KeyValuePair<string, double> resource in left)
        {
            if (!right.TryGetValue(resource.Key, out double rightValue)
                || !resource.Value.Equals(rightValue))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Builder for <see cref="SlotSharingGroup"/>.
    /// </summary>
    [Experimental]
    public class Builder
    {
        private readonly string _name;
        private double? _cpuCores;
        private MemorySize? _taskHeapMemory;
        private MemorySize? _taskOffHeapMemory;
        private MemorySize? _managedMemory;
        private readonly Dictionary<string, double> _externalResources = new();

        internal Builder(string name)
        {
            _name = name;
        }

        /// <summary>Set the CPU cores for this SlotSharingGroup.</summary>
        public Builder SetCpuCores(double cpuCores)
        {
            if (cpuCores <= 0)
            {
                throw new ArgumentException("The cpu cores should be positive.");
            }
            _cpuCores = cpuCores;
            return this;
        }

        /// <summary>Set the task heap memory for this SlotSharingGroup.</summary>
        public Builder SetTaskHeapMemory(MemorySize taskHeapMemory)
        {
            if (taskHeapMemory.CompareTo(MemorySize.Zero) <= 0)
            {
                throw new ArgumentException("The task heap memory should be positive.");
            }
            _taskHeapMemory = taskHeapMemory;
            return this;
        }

        /// <summary>Set the task heap memory for this SlotSharingGroup in MB.</summary>
        public Builder SetTaskHeapMemoryMB(int taskHeapMemoryMB)
        {
            if (taskHeapMemoryMB <= 0)
            {
                throw new ArgumentException("The task heap memory should be positive.");
            }
            _taskHeapMemory = MemorySize.OfMebiBytes(taskHeapMemoryMB);
            return this;
        }

        /// <summary>Set the task off-heap memory for this SlotSharingGroup.</summary>
        public Builder SetTaskOffHeapMemory(MemorySize taskOffHeapMemory)
        {
            _taskOffHeapMemory = taskOffHeapMemory;
            return this;
        }

        /// <summary>Set the task off-heap memory for this SlotSharingGroup in MB.</summary>
        public Builder SetTaskOffHeapMemoryMB(int taskOffHeapMemoryMB)
        {
            _taskOffHeapMemory = MemorySize.OfMebiBytes(taskOffHeapMemoryMB);
            return this;
        }

        /// <summary>Set the task managed memory for this SlotSharingGroup.</summary>
        public Builder SetManagedMemory(MemorySize managedMemory)
        {
            _managedMemory = managedMemory;
            return this;
        }

        /// <summary>Set the task managed memory for this SlotSharingGroup in MB.</summary>
        public Builder SetManagedMemoryMB(int managedMemoryMB)
        {
            _managedMemory = MemorySize.OfMebiBytes(managedMemoryMB);
            return this;
        }

        /// <summary>
        /// Add the given external resource. The old value with the same resource name will be
        /// replaced if present.
        /// </summary>
        public Builder SetExternalResource(string name, double value)
        {
            _externalResources[name] = value;
            return this;
        }

        /// <summary>Build the SlotSharingGroup.</summary>
        public SlotSharingGroup Build()
        {
            if (_cpuCores != null && _taskHeapMemory != null)
            {
                _taskOffHeapMemory ??= MemorySize.Zero;
                _managedMemory ??= MemorySize.Zero;
                return new SlotSharingGroup(
                    _name,
                    _cpuCores,
                    _taskHeapMemory,
                    _taskOffHeapMemory,
                    _managedMemory,
                    _externalResources);
            }
            else if (_cpuCores != null
                || _taskHeapMemory != null
                || _taskOffHeapMemory != null
                || _managedMemory != null
                || _externalResources.Count != 0)
            {
                throw new ArgumentException(
                    "The cpu cores and task heap memory are required when specifying the resource of a slot sharing group. "
                        + "You need to explicitly configure them with positive value.");
            }
            else
            {
                return new SlotSharingGroup(_name);
            }
        }
    }
}
