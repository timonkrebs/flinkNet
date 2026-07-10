#!/usr/bin/env python3
# Licensed to the Apache Software Foundation (ASF) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The ASF licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""C# port of org.apache.flink.api.java.tuple.TupleGenerator.

Generates Tuple.cs, Tuple0.cs..Tuple25.cs and the corresponding builder
classes. Run from the dotnet/ directory:

    python3 tools/generate_tuples.py

Edit this generator, never the generated files.
"""

import os

MAX_ARITY = 25

ROOT = os.path.join(os.path.dirname(__file__), "..", "src", "FlinkNet.Core.Api")
TUPLE_DIR = os.path.join(ROOT, "Api", "Tuples")
BUILDER_DIR = os.path.join(TUPLE_DIR, "Builders")

LICENSE = """\
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

// --------------------------------------------------------------
//  THIS IS A GENERATED SOURCE FILE. DO NOT EDIT!
//  GENERATED FROM tools/generate_tuples.py
//  (C# port of org.apache.flink.api.java.tuple.TupleGenerator)
// --------------------------------------------------------------
"""


def type_params(arity):
    return ", ".join(f"T{i}" for i in range(arity))


def write(path, content):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)
    print(f"generated {os.path.relpath(path, os.path.join(ROOT, '..', '..'))}")


def gen_tuple_base():
    new_instance_cases = "\n".join(
        f"            {i} => new Tuple{i}<{', '.join(['object'] * i)}>()," if i > 0
        else "            0 => Tuple0.Instance,"
        for i in range(MAX_ARITY + 1)
    )
    classes = "\n".join(
        f"        typeof(Tuple{i}<{',' * (i - 1)}>)," if i > 0 else "        typeof(Tuple0),"
        for i in range(MAX_ARITY + 1)
    )

    content = f"""{LICENSE}
using System.Text;
using FlinkNet.Annotations;
using FlinkNet.Types;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// The base class of all tuples. Tuples have a fix length and contain a set of fields, which may
/// all be of different types. Because Tuples are strongly typed, each distinct tuple length is
/// represented by its own class. Tuples exists with up to 25 fields and are described in the
/// classes <see cref="Tuple1{{T0}}"/> to Tuple25.
///
/// <para>The fields in the tuples may be accessed directly as public fields, or via position
/// (zero indexed) <see cref="GetField{{T}}(int)"/>.</para>
/// </summary>
[Public]
public abstract class Tuple
{{
    public const int MaxArity = {MAX_ARITY};

    /// <summary>Gets the field at the specified position.</summary>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <returns>The field at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    public abstract T? GetField<T>(int pos);

    /// <summary>
    /// Gets the field at the specified position, throws <see cref="NullFieldException"/> if the
    /// field is null. Used for comparing key fields.
    /// </summary>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <returns>The field at the specified position.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    /// <exception cref="NullFieldException">Thrown, if the field at pos is null.</exception>
    public T GetFieldNotNull<T>(int pos)
    {{
        T? field = GetField<T>(pos);
        return field is not null ? field : throw new NullFieldException(pos);
    }}

    /// <summary>Sets the field at the specified position.</summary>
    /// <param name="value">The value to be assigned to the field at the specified position.</param>
    /// <param name="pos">The position of the field, zero indexed.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown, if the position is negative, or equal
    /// to, or larger than the number of fields.</exception>
    public abstract void SetField<T>(T? value, int pos);

    /// <summary>Gets the number of fields in the tuple (the tuple arity).</summary>
    public abstract int Arity {{ get; }}

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public abstract Tuple Copy();

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Gets the type corresponding to the tuple of the given arity (dimensions). For example,
    /// <c>GetTupleType(3)</c> will return the open generic <c>Tuple3&lt;,,&gt;</c> type.
    /// </summary>
    /// <param name="arity">The arity of the tuple type to get.</param>
    /// <returns>The tuple type with the given arity.</returns>
    public static Type GetTupleType(int arity)
    {{
        if (arity < 0 || arity > MaxArity)
        {{
            throw new ArgumentException($"The tuple arity must be in [0, {{MaxArity}}].");
        }}
        return Types[arity];
    }}

    /// <summary>
    /// Creates a new tuple of the given arity, with all field types being <c>object</c> and all
    /// fields null (Tuple0 returns the shared <see cref="Tuple0.Instance"/>).
    /// </summary>
    public static Tuple NewInstance(int arity) =>
        arity switch
        {{
{new_instance_cases}
            _ => throw new ArgumentException($"The tuple arity must be in [0, {{MaxArity}}]."),
        }};

    private static readonly Type[] Types =
    [
{classes}
    ];

    /// <summary>
    /// Converts the given object into a string representation by calling
    /// <see cref="object.ToString"/> and formatting (possibly nested) arrays and <c>null</c>.
    /// Mirrors the format of Java's <c>Arrays.deepToString</c>.
    /// </summary>
    internal static string ArrayAwareToString(object? o)
    {{
        if (o is null)
        {{
            return "null";
        }}

        if (o is Array array)
        {{
            var builder = new StringBuilder("[");
            bool first = true;
            foreach (object? element in array)
            {{
                if (!first)
                {{
                    builder.Append(", ");
                }}
                first = false;
                builder.Append(ArrayAwareToString(element));
            }}
            return builder.Append(']').ToString();
        }}

        return o.ToString() ?? "null";
    }}

    /// <summary>
    /// Whether the object is an instantiation (or a subclass of an instantiation) of the given
    /// open generic tuple class. PORT NOTE: replicates Java's raw <c>instanceof TupleN</c>
    /// checks, which compare tuples structurally across generic instantiations.
    /// </summary>
    private protected static bool IsSameTupleClass(object? obj, Type openTupleType)
    {{
        for (Type? type = obj?.GetType(); type != null; type = type.BaseType)
        {{
            if (type.IsGenericType && type.GetGenericTypeDefinition() == openTupleType)
            {{
                return true;
            }}
        }}
        return false;
    }}
}}
"""
    write(os.path.join(TUPLE_DIR, "Tuple.cs"), content)


def gen_tuple0():
    content = f"""{LICENSE}
using FlinkNet.Annotations;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// A tuple with 0 fields.
///
/// <para>The Tuple0 is a soft singleton, i.e., there is a "singleton" instance, but it does not
/// prevent creation of additional instances.</para>
/// </summary>
/// <seealso cref="Tuple"/>
[Public]
public class Tuple0 : Tuple
{{
    /// <summary>An immutable reusable Tuple0 instance.</summary>
    public static readonly Tuple0 Instance = new();

    // ------------------------------------------------------------------------

    public override int Arity => 0;

    public override T? GetField<T>(int pos) where T : default =>
        throw new IndexOutOfRangeException(pos.ToString());

    public override void SetField<T>(T? value, int pos) where T : default =>
        throw new IndexOutOfRangeException(pos.ToString());

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple0 Copy() => new();

    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>Creates a string representation of the tuple in the form "()".</summary>
    public override string ToString() => "()";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Tuple0;

    public override int GetHashCode() => 0;
}}
"""
    write(os.path.join(TUPLE_DIR, "Tuple0.cs"), content)


def gen_tuple(arity):
    tps = type_params(arity)
    open_generic_commas = "," * (arity - 1)
    fields_doc = "\n".join(
        f"/// <typeparam name=\"T{i}\">The type of field {i}</typeparam>" for i in range(arity)
    )

    field_decls = "\n\n".join(
        f"    /// <summary>Field {i} of the tuple.</summary>\n    public T{i}? F{i};"
        for i in range(arity)
    )

    ctor_params = ", ".join(f"T{i}? f{i}" for i in range(arity))
    ctor_param_docs = "\n".join(
        f"    /// <param name=\"f{i}\">The value for field {i}</param>" for i in range(arity)
    )
    ctor_assignments = "\n".join(f"        F{i} = f{i};" for i in range(arity))

    get_cases = "\n".join(
        f"            {i} => (T?)(object?)F{i}," for i in range(arity)
    )
    set_cases = "\n".join(
        f"""            case {i}:
                F{i} = (T{i}?)(object?)value;
                break;"""
        for i in range(arity)
    )

    to_string_parts = ' + "," + '.join(f"ArrayAwareToString(F{i})" for i in range(arity))

    equals_checks = "\n".join(
        f"""        if (!Equals(F{i}, tuple.GetField<object>({i})))
        {{
            return false;
        }}"""
        for i in range(arity)
    )

    hash_lines = ["        int result = F0?.GetHashCode() ?? 0;"] + [
        f"        result = 31 * result + (F{i}?.GetHashCode() ?? 0);" for i in range(1, arity)
    ]
    hash_code = "\n".join(hash_lines)

    copy_args = ", ".join(f"F{i}" for i in range(arity))
    of_params = ", ".join(f"T{i}? f{i}" for i in range(arity))
    of_args = ", ".join(f"f{i}" for i in range(arity))

    swap = ""
    if arity == 2:
        swap = """
    /// <summary>Returns a shallow copy of the tuple with swapped values.</summary>
    /// <returns>shallow copy of the tuple with swapped values</returns>
    public Tuple2<T1, T0> Swap() => new(F1, F0);
"""

    content = f"""{LICENSE}
using FlinkNet.Annotations;

namespace FlinkNet.Api.Tuples;

/// <summary>
/// A tuple with {arity} field{"s" if arity != 1 else ""}. Tuples are strongly typed; each field may be of a separate type. The
/// fields of the tuple can be accessed directly as public fields (F0, F1, ...) or via their
/// position through the <see cref="GetField{{T}}(int)"/> method. The tuple field positions start
/// at zero.
///
/// <para>Tuples are mutable types, meaning that their fields can be re-assigned. This allows
/// functions that work with Tuples to reuse objects in order to reduce pressure on the garbage
/// collector.</para>
///
/// <para>Warning: If you subclass Tuple{arity}, then be sure to either
/// <list type="bullet">
///   <item><description>not add any new fields, or</description></item>
///   <item><description>make it a POJO, and always declare the element type of your
///   DataStreams to your descendant type.</description></item>
/// </list></para>
/// </summary>
/// <seealso cref="Tuple"/>
{fields_doc}
[Public]
public class Tuple{arity}<{tps}> : Tuple
{{
{field_decls}

    /// <summary>Creates a new tuple where all fields are null.</summary>
    public Tuple{arity}() {{ }}

    /// <summary>Creates a new tuple and assigns the given values to the tuple's fields.</summary>
{ctor_param_docs}
    public Tuple{arity}({ctor_params})
    {{
{ctor_assignments}
    }}

    public override int Arity => {arity};

    public override T? GetField<T>(int pos) where T : default =>
        pos switch
        {{
{get_cases}
            _ => throw new IndexOutOfRangeException(pos.ToString()),
        }};

    public override void SetField<T>(T? value, int pos) where T : default
    {{
        switch (pos)
        {{
{set_cases}
            default:
                throw new IndexOutOfRangeException(pos.ToString());
        }}
    }}

    /// <summary>Sets new values to all fields of the tuple.</summary>
{ctor_param_docs}
    public void SetFields({ctor_params})
    {{
{ctor_assignments}
    }}
{swap}
    // -------------------------------------------------------------------------------------------------
    // standard utilities
    // -------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a string representation of the tuple in the form ({", ".join(f"f{i}" for i in range(arity))}), where the
    /// individual fields are the value returned by calling <see cref="object.ToString"/> on that
    /// field.
    /// </summary>
    public override string ToString() =>
        "(" + {to_string_parts} + ")";

    /// <summary>Deep equality for tuples by calling Equals() on the tuple members. Like Java's
    /// raw <c>instanceof</c> check, tuples of the same arity class compare structurally across
    /// generic instantiations.</summary>
    /// <param name="obj">the object checked for equality</param>
    /// <returns>true if this is equal to <paramref name="obj"/>.</returns>
    public override bool Equals(object? obj)
    {{
        if (ReferenceEquals(this, obj))
        {{
            return true;
        }}
        if (!IsSameTupleClass(obj, typeof(Tuple{arity}<{open_generic_commas}>)))
        {{
            return false;
        }}
        var tuple = (Tuple)obj!;
{equals_checks}
        return true;
    }}

    public override int GetHashCode()
    {{
{hash_code}
        return result;
    }}

    /// <summary>Shallow tuple copy.</summary>
    /// <returns>A new Tuple with the same fields as this.</returns>
    public override Tuple{arity}<{tps}> Copy() => new({copy_args});
}}

/// <summary>
/// Factory for <see cref="Tuple{arity}{{{tps}}}"/> allowing the compiler to infer the generic type
/// arguments implicitly. For example: <c>Tuple{arity}.Of(...)</c> instead of
/// <c>new Tuple{arity}&lt;...&gt;(...)</c>.
/// </summary>
[Public]
public static class Tuple{arity}
{{
    /// <summary>
    /// Creates a new tuple and assigns the given values to the tuple's fields. This is more
    /// convenient than using the constructor, because the compiler can infer the generic type
    /// arguments implicitly.
    /// </summary>
    public static Tuple{arity}<{tps}> Of<{tps}>({of_params}) => new({of_args});
}}
"""
    write(os.path.join(TUPLE_DIR, f"Tuple{arity}.cs"), content)


def gen_builder(arity):
    if arity == 0:
        add_params = ""
        add_body = "Tuple0.Instance"
        tuple_type = "Tuple0"
        class_decl = "Tuple0Builder"
        type_doc = ""
    else:
        tps = type_params(arity)
        add_params = ", ".join(f"T{i}? f{i}" for i in range(arity))
        add_body = f"new Tuple{arity}<{tps}>({', '.join(f'f{i}' for i in range(arity))})"
        tuple_type = f"Tuple{arity}<{tps}>"
        class_decl = f"Tuple{arity}Builder<{tps}>"
        type_doc = "\n" + "\n".join(
            f"/// <typeparam name=\"T{i}\">The type of field {i}</typeparam>" for i in range(arity)
        )

    content = f"""{LICENSE}
using FlinkNet.Annotations;

namespace FlinkNet.Api.Tuples.Builders;

/// <summary>
/// A builder class for <see cref="Tuple{arity}{"{" + type_params(arity) + "}" if arity > 0 else ""}"/>.
/// </summary>{type_doc}
[Public]
public class {class_decl}
{{
    private readonly List<{tuple_type}> _tuples = [];

    public {class_decl} Add({add_params})
    {{
        _tuples.Add({add_body});
        return this;
    }}

    public {tuple_type}[] Build() => _tuples.ToArray();
}}
"""
    write(os.path.join(BUILDER_DIR, f"Tuple{arity}Builder.cs"), content)


def main():
    os.makedirs(TUPLE_DIR, exist_ok=True)
    os.makedirs(BUILDER_DIR, exist_ok=True)
    gen_tuple_base()
    gen_tuple0()
    for arity in range(1, MAX_ARITY + 1):
        gen_tuple(arity)
    for arity in range(0, MAX_ARITY + 1):
        gen_builder(arity)


if __name__ == "__main__":
    main()
