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
using FlinkNet.Core.Memory;

namespace FlinkNet.Util;

/// <summary>
/// Utility methods for persisting and resolving type names.
///
/// <para>PORT NOTE: covers the class-name parts of Java's <c>InstantiationUtil</c>
/// (<c>resolveClassByName</c>). Java persists bare class names; the CLR equivalent is a
/// namespace-qualified name plus a <em>simple</em> assembly name, applied recursively to generic
/// arguments. Assembly version, culture and public key token are deliberately omitted: the
/// runtime refuses to bind an assembly older than a requested version, so persisting versions
/// would make snapshots written by a newer build unreadable after a rollback.</para>
/// </summary>
[Internal]
public static class InstantiationUtil
{
    /// <summary>
    /// Returns a version-independent, assembly-qualified name of the given type, e.g.
    /// <c>System.Collections.Generic.List`1[[System.String, System.Private.CoreLib]],
    /// System.Private.CoreLib</c>. The name resolves via <see cref="Type.GetType(string)"/>.
    /// </summary>
    public static string GetPortableTypeName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return QualifiedName(type) + ", " + type.Assembly.GetName().Name;
    }

    /// <summary>Writes the portable name of the given type (see
    /// <see cref="GetPortableTypeName"/>) in modified UTF-8.</summary>
    public static void WriteTypeName(IDataOutputView output, Type type) =>
        output.WriteUTF(GetPortableTypeName(type));

    /// <summary>
    /// Reads a type name written by <see cref="WriteTypeName"/> and resolves it. Names carrying
    /// full assembly identities also resolve.
    /// </summary>
    /// <exception cref="IOException">If the type cannot be found in the current runtime.</exception>
    public static Type ResolveTypeByName(IDataInputView input)
    {
        string typeName = input.ReadUTF();
        try
        {
            return Type.GetType(typeName, throwOnError: true)!;
        }
        catch (Exception e) when (e is TypeLoadException or FileNotFoundException
            or FileLoadException or BadImageFormatException or ArgumentException)
        {
            throw new IOException(
                "Could not find class '" + typeName + "' in the current runtime.", e);
        }
    }

    private static string QualifiedName(Type type)
    {
        if (type.IsArray)
        {
            string rank = type.IsSZArray ? "[]" : "[" + new string(',', type.GetArrayRank() - 1) + "]";
            return QualifiedName(type.GetElementType()!) + rank;
        }

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            IEnumerable<string> arguments =
                type.GetGenericArguments().Select(a => "[" + GetPortableTypeName(a) + "]");
            return type.GetGenericTypeDefinition().FullName + "[" + string.Join(",", arguments) + "]";
        }

        return type.FullName
            ?? throw new ArgumentException("Type " + type + " has no persistable name.");
    }
}
