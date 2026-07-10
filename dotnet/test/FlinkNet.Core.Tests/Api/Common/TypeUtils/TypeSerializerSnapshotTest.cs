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

using FlinkNet.Api.Common.TypeUtils;
using FlinkNet.Api.Common.TypeUtils.Base;
using FlinkNet.Api.Tuples;
using FlinkNet.Api.TypeUtils.Runtime;
using FlinkNet.Core.Memory;
using Xunit;

namespace FlinkNet.Tests.Api.Common.TypeUtils;

/// <summary>
/// Tests for the serializer snapshot machinery: versioned snapshot round-trips, schema
/// compatibility resolution, and the composite snapshot format.
/// </summary>
public class TypeSerializerSnapshotTest
{
    private static TypeSerializerSnapshot WriteAndRead(TypeSerializerSnapshot snapshot)
    {
        var output = new DataOutputSerializer(128);
        TypeSerializerSnapshot.WriteVersionedSnapshot(output, snapshot);
        return TypeSerializerSnapshot.ReadVersionedSnapshot(
            new DataInputDeserializer(output.GetCopyOfBuffer()));
    }

    [Fact]
    public void TestSimpleSnapshotRoundTrip()
    {
        TypeSerializerSnapshot restored =
            WriteAndRead(IntSerializer.Instance.SnapshotConfiguration());

        var typed = Assert.IsType<IntSerializer.IntSerializerSnapshot>(restored);
        Assert.Same(IntSerializer.Instance, typed.RestoreSerializer());
    }

    [Fact]
    public void TestAllBasicSerializerSnapshotsRoundTrip()
    {
        TypeSerializer[] serializers =
        [
            BooleanSerializer.Instance,
            ByteSerializer.Instance,
            CharSerializer.Instance,
            DoubleSerializer.Instance,
            FloatSerializer.Instance,
            IntSerializer.Instance,
            LongSerializer.Instance,
            ShortSerializer.Instance,
            StringSerializer.Instance,
        ];

        foreach (TypeSerializer serializer in serializers)
        {
            TypeSerializerSnapshot restored =
                WriteAndRead(serializer.UntypedSnapshotConfiguration());
            Assert.Same(serializer, restored.RestoreSerializerUntyped());
        }
    }

    [Fact]
    public void TestSimpleSnapshotCompatibility()
    {
        TypeSerializerSnapshot<int> newSnapshot = IntSerializer.Instance.SnapshotConfiguration();
        TypeSerializerSnapshot<int> oldSnapshot = IntSerializer.Instance.SnapshotConfiguration();

        Assert.True(newSnapshot.ResolveSchemaCompatibility(oldSnapshot).IsCompatibleAsIs());
    }

    [Fact]
    public void TestListSnapshotRoundTripAndCompatibility()
    {
        var serializer = new ListSerializer<string>(StringSerializer.Instance);

        TypeSerializerSnapshot restored = WriteAndRead(serializer.SnapshotConfiguration());
        var typed = Assert.IsType<ListSerializerSnapshot<string>>(restored);

        // the restored serializer reads data written by the original serializer
        var restoredSerializer = (ListSerializer<string>)typed.RestoreSerializer();
        Assert.Equal(serializer, restoredSerializer);

        var output = new DataOutputSerializer(64);
        serializer.Serialize(["beep", "boop"], output);
        IList<string> deserialized =
            restoredSerializer.Deserialize(new DataInputDeserializer(output.GetCopyOfBuffer()));
        Assert.Equal(["beep", "boop"], deserialized);

        // a snapshot of the same configuration is compatible as is
        Assert.True(
            serializer
                .SnapshotConfiguration()
                .ResolveSchemaCompatibility(typed)
                .IsCompatibleAsIs());
    }

    [Fact]
    public void TestMapSnapshotRoundTrip()
    {
        var serializer = new MapSerializer<string, long>(
            StringSerializer.Instance, LongSerializer.Instance);

        TypeSerializerSnapshot restored = WriteAndRead(serializer.SnapshotConfiguration());
        var typed = Assert.IsType<MapSerializerSnapshot<string, long>>(restored);

        Assert.IsType<StringSerializer.StringSerializerSnapshot>(typed.GetKeySerializerSnapshot());
        Assert.IsType<LongSerializer.LongSerializerSnapshot>(typed.GetValueSerializerSnapshot());
        Assert.Equal(serializer, typed.RestoreSerializer());
    }

    [Fact]
    public void TestTupleSnapshotRoundTrip()
    {
        TupleSerializer<Tuple2<int, string>> serializer =
            TupleSerializer<Tuple2<int, string>>.ForFields(
                IntSerializer.Instance, StringSerializer.Instance);

        TypeSerializerSnapshot restored = WriteAndRead(serializer.SnapshotConfiguration());
        var typed = Assert.IsType<TupleSerializerSnapshot<Tuple2<int, string>>>(restored);

        var restoredSerializer = (TupleSerializer<Tuple2<int, string>>)typed.RestoreSerializer();
        Assert.Equal(serializer, restoredSerializer);

        var output = new DataOutputSerializer(64);
        serializer.Serialize(Tuple2.Of(42, "hello"), output);
        Tuple2<int, string> deserialized =
            restoredSerializer.Deserialize(new DataInputDeserializer(output.GetCopyOfBuffer()));
        Assert.Equal(42, deserialized.F0);
        Assert.Equal("hello", deserialized.F1);
    }

    [Fact]
    public void TestMismatchedSnapshotsAreIncompatible()
    {
        // different serializer classes over the same type
        var intCompatibility = IntSerializer.Instance
            .SnapshotConfiguration()
            .ResolveSchemaCompatibility(new AlternativeIntSerializer().SnapshotConfiguration());
        Assert.True(intCompatibility.IsIncompatible());

        // composite with different nested serializer (same arity)
        var stringList = new ListSerializer<string>(StringSerializer.Instance);
        var otherList = new ListSerializer<int>(IntSerializer.Instance);
        TypeSerializerSchemaCompatibility compatibility =
            ((TypeSerializerSnapshot)stringList.SnapshotConfiguration())
                .ResolveSchemaCompatibilityUntyped(otherList.SnapshotConfiguration());
        Assert.True(compatibility.IsIncompatible());
    }

    [Fact]
    public void TestSerializationUtilProxyRoundTrip()
    {
        var serializer = new ListSerializer<int>(IntSerializer.Instance);

        var output = new DataOutputSerializer(128);
        TypeSerializerSnapshotSerializationUtil.WriteSerializerSnapshot(
            output, serializer.SnapshotConfiguration());

        TypeSerializerSnapshot restored =
            TypeSerializerSnapshotSerializationUtil.ReadSerializerSnapshot(
                new DataInputDeserializer(output.GetCopyOfBuffer()));

        Assert.IsType<ListSerializerSnapshot<int>>(restored);
        Assert.Equal(serializer, restored.RestoreSerializerUntyped());
    }

    [Fact]
    public void TestCompositeSnapshotRejectsCorruptMagicNumber()
    {
        var serializer = new ListSerializer<int>(IntSerializer.Instance);
        TypeSerializerSnapshot<IList<int>> snapshot = serializer.SnapshotConfiguration();

        var output = new DataOutputSerializer(128);
        snapshot.WriteSnapshot(output);
        byte[] bytes = output.GetCopyOfBuffer();
        bytes[0] ^= 0x55; // corrupt the leading magic number

        var readSnapshot = new ListSerializerSnapshot<int>();
        Assert.Throws<IOException>(
            () => readSnapshot.ReadSnapshot(
                snapshot.CurrentVersion, new DataInputDeserializer(bytes)));
    }

    [Fact]
    public void TestIntermediateCompatibilityResult()
    {
        TypeSerializerSnapshot[] newSnapshots =
            TypeSerializerUtils.Snapshot(IntSerializer.Instance, StringSerializer.Instance);
        TypeSerializerSnapshot[] oldSnapshots =
            TypeSerializerUtils.Snapshot(IntSerializer.Instance, StringSerializer.Instance);

        var result = CompositeTypeSerializerUtil.ConstructIntermediateCompatibilityResult<object>(
            newSnapshots, oldSnapshots);
        Assert.True(result.IsCompatibleAsIs());
        Assert.True(result.GetFinalResult().IsCompatibleAsIs());
        Assert.Equal(2, result.GetNestedSerializers().Length);

        // one incompatible nested pair short-circuits the whole result
        TypeSerializerSnapshot[] mismatched =
            TypeSerializerUtils.Snapshot(IntSerializer.Instance, LongSerializer.Instance);
        var incompatible =
            CompositeTypeSerializerUtil.ConstructIntermediateCompatibilityResult<object>(
                newSnapshots, mismatched);
        Assert.True(incompatible.IsIncompatible());
    }

    /// <summary>A second int serializer, distinct from <see cref="IntSerializer"/>, to pin
    /// incompatibility between different serializer classes over the same type.</summary>
    private sealed class AlternativeIntSerializer : TypeSerializerSingleton<int>
    {
        public static readonly AlternativeIntSerializer Instance = new();

        public override bool IsImmutableType => true;

        public override int CreateInstance() => 0;

        public override int Copy(int from) => from;

        public override int Copy(int from, int reuse) => from;

        public override int Length => 4;

        public override void Serialize(int record, IDataOutputView target) =>
            target.WriteInt(record);

        public override int Deserialize(IDataInputView source) => source.ReadInt();

        public override int Deserialize(int reuse, IDataInputView source) => Deserialize(source);

        public override void Copy(IDataInputView source, IDataOutputView target) =>
            target.WriteInt(source.ReadInt());

        public override TypeSerializerSnapshot<int> SnapshotConfiguration() =>
            new AlternativeIntSerializerSnapshot();

        public sealed class AlternativeIntSerializerSnapshot : SimpleTypeSerializerSnapshot<int>
        {
            public AlternativeIntSerializerSnapshot()
                : base(() => Instance)
            {
            }
        }
    }
}
