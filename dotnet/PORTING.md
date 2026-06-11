# FlinkNet — Porting Apache Flink from Java to .NET

This directory contains **FlinkNet**, an incremental port of Apache Flink to .NET (C#).
The original Java sources stay in the repository root and serve as the porting
reference; modules are ported bottom-up along the Maven dependency graph and the
Java tree is removed only once the port is complete.

## Status

| Flink module (Java)    | .NET project                              | Status      |
|------------------------|-------------------------------------------|-------------|
| `flink-annotations`    | `src/FlinkNet.Annotations`                | ✅ ported   |
| `flink-core-api`       | `src/FlinkNet.Core.Api`                   | ✅ ported   |
| `flink-core`           | `src/FlinkNet.Core`                       | 🟨 in progress |
| `flink-metrics-core`   | `src/FlinkNet.Metrics.Core` (planned)     | ⬜          |
| `flink-datastream-api` | `src/FlinkNet.DataStream.Api` (planned)   | ⬜          |
| `flink-rpc`            | `src/FlinkNet.Rpc` (planned)              | ⬜          |
| `flink-runtime`        | `src/FlinkNet.Runtime` (planned)          | ⬜          |
| `flink-streaming-java` | `src/FlinkNet.Streaming` (planned)        | ⬜          |
| `flink-clients`        | `src/FlinkNet.Clients` (planned)          | ⬜          |
| `flink-table/*`        | `src/FlinkNet.Table.*` (planned)          | ⬜          |

Build and test everything with:

```bash
cd dotnet
dotnet build
dotnet test
```

## Project layout

- One .NET project per Flink Maven module, named `FlinkNet.<Module>`
  (e.g. `flink-core-api` → `FlinkNet.Core.Api`).
- Tests go to `test/FlinkNet.<Module>.Tests` using xUnit, one test class per
  ported Java test class.
- Directory structure inside a project mirrors the namespace below the
  `FlinkNet` root (`RootNamespace` is `FlinkNet`).

## Naming and namespace mapping

| Java package                              | C# namespace                      |
|-------------------------------------------|-----------------------------------|
| `org.apache.flink`                        | `FlinkNet`                        |
| `org.apache.flink.annotation`             | `FlinkNet.Annotations`            |
| `org.apache.flink.annotation.docs`        | `FlinkNet.Annotations.Docs`       |
| `org.apache.flink.api.common`             | `FlinkNet.Api.Common`             |
| `org.apache.flink.api.common.functions`   | `FlinkNet.Api.Common.Functions`   |
| `org.apache.flink.api.common.state`       | `FlinkNet.Api.Common.State`       |
| `org.apache.flink.api.common.state.v2`    | `FlinkNet.Api.Common.State.V2`    |
| `org.apache.flink.api.common.typeinfo`    | `FlinkNet.Api.Common.TypeInfo`    |
| `org.apache.flink.api.common.watermark`   | `FlinkNet.Api.Common.Watermarks`  |
| `org.apache.flink.api.connector.dsv2`     | `FlinkNet.Api.Connector.DsV2`     |
| `org.apache.flink.api.java.functions`     | `FlinkNet.Api.Functions`          |
| `org.apache.flink.api.java.tuple`         | `FlinkNet.Api.Tuples`             |
| `org.apache.flink.configuration`          | `FlinkNet.Configuration`          |
| `org.apache.flink.types`                  | `FlinkNet.Types`                  |
| `org.apache.flink.util`                   | `FlinkNet.Util`                   |

The `java` segment of Java package names is dropped — it is meaningless in .NET.

## Language conventions

The port keeps Flink's architecture and API shape recognizable so that porting
higher layers stays mechanical, but uses .NET idioms wherever a Java construct
has a native C# equivalent:

1. **Interfaces** get the `I` prefix: `ValueState<T>` → `IValueState<T>`,
   `Function` → `IFunction`. Java default methods become C# default interface
   methods.
2. **Annotations** become attributes: `@Public` → `[Public]`
   (class `PublicAttribute`). Java `@Target` maps to `[AttributeUsage]`;
   `@Retention(RUNTIME)` needs no equivalent (CLR attributes are always
   retained).
3. **Methods/fields** become PascalCase. Simple Java getters/setters become C#
   properties (`getBytes()` → `Bytes`); getters that compute or take arguments
   stay methods.
4. **Javadoc** becomes XML doc comments: `{@link X}` → `<see cref="X"/>`,
   `<p>` → `<para>`, `{@code x}` → `<c>x</c>`, `<pre>{@code ...}</pre>` →
   `<code>...</code>`.
5. **Checked exceptions** are dropped (`throws Exception` has no C#
   equivalent). Exception type mapping:
   - `IllegalArgumentException` → `ArgumentException`
   - `IllegalStateException` → `InvalidOperationException`
   - `NullPointerException` / `Objects.requireNonNull` → `ArgumentNullException`
   - `UnsupportedOperationException` → `NotSupportedException`
   - `NumberFormatException` → `FormatException`
   - `IndexOutOfBoundsException` → `IndexOutOfRangeException`
6. **`java.io.Serializable`** is dropped. Flink serializes user functions and
   data through its own `TypeSerializer` stack, which the port will provide;
   CLR binary serialization is obsolete and is not used.
7. **Java functional interfaces** map to delegates: `Function<T,R>` →
   `Func<T,R>`, `Supplier<T>` → `Func<T>`, `Consumer<T>` → `Action<T>`,
   `BiFunction<T,U,R>` → `Func<T,U,R>`. The whole
   `org.apache.flink.util.function` package (`ThrowingConsumer`,
   `FunctionWithException`, `TriFunction`, …) is **not ported**: those types
   exist only because Java lambdas cannot throw checked exceptions or are not
   serializable; in C# plain `Func<>`/`Action<>` cover all of them.
   Flink interfaces that are themselves `@FunctionalInterface` (e.g.
   `ReduceFunction`) stay interfaces to preserve the API shape.
8. **`Optional<T>`** becomes a nullable (`T?`); `Optional<Boolean>` → `bool?`.
9. **Boxed primitives in generics** (`Tuple2<Integer, String>`) become the C#
   primitives (`Tuple2<int, string>`); generics over value types need no boxing
   in .NET.
10. **`equals`/`hashCode`/`toString`** become `Equals`/`GetHashCode`/`ToString`
    overrides with the same semantics.
11. **Locale-sensitive formatting** (`String.format(Locale.ROOT, …)`) uses
    `CultureInfo.InvariantCulture`.
12. **Generated code stays generated**: the tuple classes are produced by
    `tools/generate_tuples.py` (port of Flink's `TupleGenerator`); edit the
    generator, not the output.
13. Every file keeps the Apache License 2.0 header.

## Porting order (dependency-driven roadmap)

1. ✅ `flink-annotations`, `flink-core-api` — pure API surface, no logic.
2. 🟨 `flink-core` — configuration, filesystems, memory segments, type
   serialization core. Biggest foundational chunk (~1.3k files); ported in
   slices: `configuration` → `util` → `core.memory` → `core.fs` →
   `api.common.typeutils` → `core.io`.
   - ✅ configuration mechanism: `ConfigOption`/`ConfigOptions`,
     `Configuration` (+`UnmodifiableConfiguration`, `DelegatingConfiguration`),
     `ConfigurationUtils` conversions, `ConfigUtils` encode/decode,
     `StructuredOptionsSplitter`, `description/*`, minimal
     `GlobalConfiguration`/`SecurityOptions` (sensitivity only), and
     `util.TimeUtils` (`Duration` → `TimeSpan`).
   - Additional conventions established here: Java's
     `Optional<T> getOptional(...)` maps to the .NET try-pattern
     (`bool TryGet(..., out T)`); value-type option defaults are stored
     boxed (`null` = no default, see `ConfigOption.DefaultValueBoxed`);
     structured values use the legacy Flink 1.x string format until the
     YAML increment.
   - ⬜ next slices: `YamlParserUtils` + `GlobalConfiguration` loading
     (standard-YAML rendering; needs a YAML dependency decision), option
     catalogs (`CoreOptions`, `TaskManagerOptions`, ...) as their
     subsystems are ported, `DescribedEnum` and
     `ConfigUtils.getAllConfigOptions` (both need a C# pattern for the
     erased/raw `ConfigOption` type), binary `read`/`write` once
     `core.memory` exists, then `util` → `core.memory` → `core.fs` →
     `api.common.typeutils` → `core.io`.
3. ⬜ `flink-metrics-core`, then `flink-datastream-api`.
4. ⬜ `flink-rpc` — replace Pekko/Akka with an in-process + TCP RPC layer
   (candidates: built-in sockets + System.Threading.Channels, or gRPC).
5. ⬜ `flink-runtime` — scheduler, checkpointing, network stack (Netty →
   System.IO.Pipelines/Kestrel transport), state backends.
6. ⬜ `flink-streaming-java`, `flink-clients`, `flink-table/*`.

Non-goals for now: Scala APIs, YARN/Mesos deployments, the web UI, and
connectors living outside this repository.
