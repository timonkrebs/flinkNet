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

using System.Numerics;

namespace FlinkNet.Util;

/// <summary>Collection of simple mathematical routines.</summary>
public static class MathUtils
{
    /// <summary>
    /// Computes the logarithm of the given value to the base of 2, rounded down. It corresponds
    /// to the position of the highest non-zero bit. The position is counted, starting with 0 from
    /// the least significant bit to the most significant bit. For example, <c>log2floor(16) = 4</c>
    /// and <c>log2floor(10) = 3</c>.
    /// </summary>
    /// <param name="value">The value to compute the logarithm for.</param>
    /// <returns>The logarithm (rounded down) to the base of 2.</returns>
    /// <exception cref="ArithmeticException">Thrown, if the given value is zero.</exception>
    public static int Log2Floor(int value)
    {
        if (value == 0)
        {
            throw new ArithmeticException("Logarithm of zero is undefined.");
        }

        return 31 - BitOperations.LeadingZeroCount((uint)value);
    }

    /// <summary>
    /// Computes the logarithm of the given value to the base of 2. This method throws an error,
    /// if the given argument is not a power of 2.
    /// </summary>
    /// <param name="value">The value to compute the logarithm for.</param>
    /// <returns>The logarithm to the base of 2.</returns>
    /// <exception cref="ArithmeticException">Thrown, if the given value is zero.</exception>
    /// <exception cref="ArgumentException">Thrown, if the given value is not a power of two.</exception>
    public static int Log2Strict(int value)
    {
        if (value == 0)
        {
            throw new ArithmeticException("Logarithm of zero is undefined.");
        }
        if ((value & (value - 1)) != 0)
        {
            throw new ArgumentException("The given value " + value + " is not a power of two.");
        }
        return 31 - BitOperations.LeadingZeroCount((uint)value);
    }

    /// <summary>
    /// Decrements the given number down to the closest power of two. If the argument is a power
    /// of two, it remains unchanged.
    /// </summary>
    /// <param name="value">The value to round down.</param>
    /// <returns>The closest value that is a power of two and less or equal than the given value.</returns>
    public static int RoundDownToPowerOf2(int value) =>
        value == 0 ? 0 : 1 << (31 - BitOperations.LeadingZeroCount((uint)value));

    /// <summary>
    /// Casts the given value to a 32 bit integer, if it can be safely done. If the cast would
    /// change the numeric value, this method raises an exception.
    /// </summary>
    /// <param name="value">The value to be cast to an integer.</param>
    /// <returns>The given value as an integer.</returns>
    public static int CheckedDownCast(long value)
    {
        int downCast = (int)value;
        if (downCast != value)
        {
            throw new ArgumentException("Cannot downcast long value " + value + " to integer.");
        }
        return downCast;
    }

    /// <summary>
    /// Checks whether the given value is a power of two.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True, if the value is a power of two, false otherwise.</returns>
    public static bool IsPowerOf2(long value) => (value & (value - 1)) == 0;

    /// <summary>
    /// This function hashes an integer value. It is adapted from Bob Jenkins' website <a
    /// href="http://www.burtleburtle.net/bob/hash/integer.html">link</a>. The hash function has
    /// the <i>full avalanche</i> property, meaning that every bit of the value to be hashed
    /// affects every bit of the hash value.
    /// </summary>
    /// <param name="code">The integer to be hashed.</param>
    /// <returns>The non-negative hash code for the integer.</returns>
    public static int JenkinsHash(int code)
    {
        unchecked
        {
            code = (code + 0x7ed55d16) + (code << 12);
            code = (code ^ unchecked((int)0xc761c23c)) ^ (code >>> 19);
            code = (code + 0x165667b1) + (code << 5);
            code = (code + unchecked((int)0xd3a2646c)) ^ (code << 9);
            code = (code + unchecked((int)0xfd7046c5)) + (code << 3);
            code = (code ^ unchecked((int)0xb55a4f09)) ^ (code >>> 16);
            return code >= 0 ? code : -(code + 1);
        }
    }

    /// <summary>
    /// This function hashes an integer value using the MurmurHash3 algorithm, ensuring a uniform
    /// distribution of hash values.
    /// </summary>
    /// <param name="code">The integer to be hashed.</param>
    /// <returns>The non-negative hash code for the integer.</returns>
    public static int MurmurHash(int code)
    {
        unchecked
        {
            code *= (int)0xcc9e2d51;
            code = (int)BitOperations.RotateLeft((uint)code, 15);
            code *= 0x1b873593;

            code = (int)BitOperations.RotateLeft((uint)code, 13);
            code = code * 5 + unchecked((int)0xe6546b64);

            code ^= 4;
            code = BitMix(code);

            if (code >= 0)
            {
                return code;
            }
            else if (code != int.MinValue)
            {
                return -code;
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Round the given number to the next power of two.
    /// </summary>
    /// <param name="x">number to round</param>
    /// <returns>x rounded up to the next power of two</returns>
    public static int RoundUpToPowerOfTwo(int x)
    {
        x -= 1;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        return x + 1;
    }

    /// <summary>
    /// Pseudo-randomly maps a long (64-bit) to an integer (32-bit) using some bit-mixing for
    /// better distribution.
    /// </summary>
    /// <param name="value">the long (64-bit) input.</param>
    /// <returns>the bit-mixed int (32-bit) output</returns>
    public static int LongToIntWithBitMixing(long value)
    {
        unchecked
        {
            value = (value ^ (value >>> 30)) * unchecked((long)0xbf58476d1ce4e5b9L);
            value = (value ^ (value >>> 27)) * unchecked((long)0x94d049bb133111ebL);
            value ^= value >>> 31;
            return (int)value;
        }
    }

    /// <summary>
    /// Bit-mixing for pseudo-randomization of integers (e.g., to guard against bad hash
    /// functions). Implementation is from Murmur's 32 bit finalizer.
    /// </summary>
    /// <param name="value">the input value</param>
    /// <returns>the bit-mixed output value</returns>
    public static int BitMix(int value)
    {
        unchecked
        {
            value ^= value >>> 16;
            value *= unchecked((int)0x85ebca6b);
            value ^= value >>> 13;
            value *= unchecked((int)0xc2b2ae35);
            value ^= value >>> 16;
            return value;
        }
    }

    /// <summary>
    /// Flips the sign bit (most-significant-bit) of the input.
    /// </summary>
    /// <param name="value">the input value.</param>
    /// <returns>the input with a flipped sign bit (most-significant-bit).</returns>
    public static long FlipSignBit(long value) => value ^ long.MinValue;

    /// <summary>
    /// Divide and rounding up to integer. E.g., divideRoundUp(3, 2) returns 2, divideRoundUp(0, 3)
    /// returns 0.
    /// </summary>
    /// <param name="dividend">value to be divided by the divisor</param>
    /// <param name="divisor">value by which the dividend is to be divided</param>
    /// <returns>the quotient rounding up to integer</returns>
    public static int DivideRoundUp(int dividend, int divisor)
    {
        if (dividend < 0)
        {
            throw new ArgumentException("Negative dividend is not supported.");
        }
        if (divisor <= 0)
        {
            throw new ArgumentException("Negative or zero divisor is not supported.");
        }
        return dividend == 0 ? 0 : (dividend - 1) / divisor + 1;
    }
}
