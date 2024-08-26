using System.Runtime.CompilerServices;

namespace Refresh.Common.Helpers;

public static class HexHelper
{
    /// <summary>
    ///     Converts a byte array to a hex string
    /// </summary>
    /// <param name="data">The hex bytes</param>
    /// <returns>The hex string</returns>
    public static string BytesToHexString(ReadOnlySpan<byte> data)
    {
        Span<char> hexChars = stackalloc char[data.Length * 2];

        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            hexChars[i * 2] = GetHexChar(b >> 4); // High bits
            hexChars[i * 2 + 1] = GetHexChar(b & 0x0F); // Low bits
        }

        return new string(hexChars);
    }

    /// <summary>
    ///     Converts a hex string to a byte array
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static byte[] HexStringToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new FormatException("The hex string is invalid as it contains an odd number of bytes.");

        // Two hex digits per byte
        byte[] arr = new byte[hex.Length / 2];

        for (int i = 0; i < arr.Length; ++i)
            // The bitmasks may seem redundant, but from my testing it consistently improves perf by ~10%,
            // probably because the compiler/JIT knows it can omit range checks in the byte -> int cast
            arr[i] = (byte)(((GetHexVal(hex[i * 2]) << 4) & 0xF0) + (GetHexVal(hex[i * 2 + 1]) & 0x0F));

        return arr;
    }

    /// <summary>
    ///     Gets the 4 bit value of the single hex digit
    /// </summary>
    /// <param name="hex">The hex digit</param>
    /// <returns>The 4 bit value of the digit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static int GetHexVal(char hex)
    {
        int val = hex;
        return val - (val < 58 ? 48 : 87);
    }

    /// <summary>
    ///     Gets the hex digit for a 4-bit value
    /// </summary>
    /// <param name="value">The 4 bit value</param>
    /// <returns>The hex digit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static char GetHexChar(int value)
    {
        return (char)(value < 10 ? '0' + value : 'a' + value - 10);
    }
}