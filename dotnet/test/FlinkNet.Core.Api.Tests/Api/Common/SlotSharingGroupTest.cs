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

using FlinkNet.Api.Common;
using FlinkNet.Configuration;
using Xunit;

namespace FlinkNet.Tests.Api.Common;

/// <summary>Tests for <see cref="SlotSharingGroup"/>.</summary>
public class SlotSharingGroupTest
{
    [Fact]
    public void TestBuildSlotSharingGroupWithSpecificResource()
    {
        const string name = "ssg";
        MemorySize heap = MemorySize.OfMebiBytes(100);
        MemorySize offHeap = MemorySize.OfMebiBytes(200);
        MemorySize managed = MemorySize.OfMebiBytes(300);
        SlotSharingGroup slotSharingGroup =
            SlotSharingGroup.NewBuilder(name)
                .SetCpuCores(1)
                .SetTaskHeapMemory(heap)
                .SetTaskOffHeapMemory(offHeap)
                .SetManagedMemory(managed)
                .SetExternalResource("gpu", 1)
                .Build();

        Assert.Equal(name, slotSharingGroup.Name);
        Assert.Equal(1.0, slotSharingGroup.CpuCores);
        Assert.Equal(heap, slotSharingGroup.TaskHeapMemory);
        Assert.Equal(offHeap, slotSharingGroup.TaskOffHeapMemory);
        Assert.Equal(managed, slotSharingGroup.ManagedMemory);
        Assert.Equal(
            new Dictionary<string, double> { { "gpu", 1.0 } },
            slotSharingGroup.ExternalResources);
    }

    [Fact]
    public void TestBuildSlotSharingGroupWithUnknownResource()
    {
        const string name = "ssg";
        SlotSharingGroup slotSharingGroup = SlotSharingGroup.NewBuilder(name).Build();

        Assert.Equal(name, slotSharingGroup.Name);
        Assert.Null(slotSharingGroup.CpuCores);
        Assert.Null(slotSharingGroup.TaskHeapMemory);
        Assert.Null(slotSharingGroup.ManagedMemory);
        Assert.Null(slotSharingGroup.TaskOffHeapMemory);
        Assert.Empty(slotSharingGroup.ExternalResources);
    }

    [Fact]
    public void TestBuildSlotSharingGroupWithIllegalConfig()
    {
        Assert.Throws<ArgumentException>(
            () =>
                SlotSharingGroup.NewBuilder("ssg")
                    .SetCpuCores(1)
                    .SetTaskHeapMemory(MemorySize.Zero)
                    .SetTaskOffHeapMemoryMB(10)
                    .Build());
    }

    [Fact]
    public void TestBuildSlotSharingGroupWithoutAllRequiredConfig()
    {
        Assert.Throws<ArgumentException>(
            () =>
                SlotSharingGroup.NewBuilder("ssg")
                    .SetCpuCores(1)
                    .SetTaskOffHeapMemoryMB(10)
                    .Build());
    }
}
