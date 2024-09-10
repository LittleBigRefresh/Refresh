using System.Buffers;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using FastAes;
using IronCompress;

namespace Refresh.Common.Helpers;

public static class ResourceHelper
{
    public static Stream StreamFromResource(string name, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        return assembly.GetManifestResourceStream(name)!;
    }

    public static byte[] ReadResource(string name, Assembly? assembly = null)
    {
        using MemoryStream stream = new();
        StreamFromResource(name, assembly).CopyTo(stream);
        return stream.ToArray();
    }
    
    private static readonly ThreadLocal<Iron> Iron = new(() => new Iron(ArrayPool<byte>.Shared));

    public static byte[] PspEncrypt(Span<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] initialCounter = new byte[16];
        key[..8].CopyTo(initialCounter);
        
        //If the file is big enough, try to compress it
        bool compress = data.Length > 1024;

        IronCompressResult? compressResult = null;
        
        //If we want to compress the data, do so, if not, dont
        ReadOnlySpan<byte> resultData = compress
            ? (compressResult = Iron.Value!.Compress(Codec.LZO, data, null, CompressionLevel.SmallestSize)).AsSpan()
            : data;

        const int infoLength = 0x19;
        const int hashLength = 0x10;
        
        byte[] final = new byte[resultData.Length + infoLength];

        Span<byte> info = final.AsSpan()[^infoLength..];

        //Copy the result data
        resultData.CopyTo(final.AsSpan()[..^infoLength]);

        //Write the buffer length
        BinaryPrimitives.WriteUInt32LittleEndian(info[..4], (uint)data.Length);
        //Mark whether or not its compressed
        info[4] = (byte)(compress ? 1 : 0);
        BinaryPrimitives.WriteUInt32LittleEndian(info[5..], 0xFFFFFFFF);

        //Get the hash of the data and part of the info
        byte[] hash = MD5.HashData(final.AsSpan()[..(final.Length - hashLength)]);
        //Copy the hash into the end of the info
        hash.AsSpan().CopyTo(info[^hashLength..]);
        
        //Encrypt the data
        AesCtr ctr = new(key, initialCounter);
        ctr.EncryptBytes(final, final);

        //If we compressed the data, free it
        compressResult?.Dispose();

        return final;
    }

    /// <summary>
    /// Decrypt a PSP asset
    /// </summary>
    /// <param name="data">The data to decrypt</param>
    /// <param name="key">The decryption key</param>
    /// <returns>The decrypted data.</returns>
    public static byte[] PspDecrypt(Span<byte> data, ReadOnlySpan<byte> key)
    {
        byte[] initialCounter = new byte[16];
        key[..8].CopyTo(initialCounter);

        //Decrypt the data
        AesCtr ctr = new(key, initialCounter);
        ctr.DecryptBytes(data, data);
        
        //Get the data buffer
        Span<byte> buf = data[..^0x19];
        //Get the info of the file
        Span<byte> info = data[^0x19..];

        uint len = BinaryPrimitives.ReadUInt32LittleEndian(info[..4]);

        ReadOnlySpan<byte> signature = info[^0x10..];

        //If the hash in the file does not match the data contained, the file is likely corrupt.
        if (!signature.SequenceEqual(MD5.HashData(data[..^0x10]))) throw new InvalidDataException("Encrypted PSP asset hash does not match signature, likely corrupt");
        
        //If the data is not compressed, just return a copy of the raw buffer
        if (info[0x4] != 1) return buf.ToArray();

        //Decompress the data
        using IronCompressResult decompressed = Iron.Value!.Decompress(Codec.LZO, buf, (int)len);

        //Return a copy of the decompressed data
        return decompressed.AsSpan().ToArray();
    }
    
    static int XXTEA_DELTA = Unsafe.BitCast<uint, int>(0x9e3779b9);

    /// <summary>
    /// In-place encrypts byte data using big endian XXTEA.
    ///
    /// Due to how XXTEA data works, you must pad the data to a multiple of 4 bytes.
    /// </summary>
    /// <param name="byteData">The data to encrypt</param>
    /// <param name="key">The key used to encrypt the data</param>
    /// <exception cref="ArgumentException">The input is not a multiple of 4 bytes</exception>
    /// <remarks>
    /// Referenced from https://github.com/ennuo/toolkit/blob/dc82bee57ab58e9f4bf35993d405529d4cbc7d00/lib/cwlib/src/main/java/cwlib/util/Crypto.java#L97
    /// </remarks>
    public static void XxteaEncrypt(Span<byte> byteData, Span<int> key)
    {
        if (byteData.Length % 4 != 0)
            throw new ArgumentException("Data must be padded to a multiple of 4 bytes.", nameof(byteData));

        // Alias the byte data as integers
        Span<int> data = MemoryMarshal.Cast<byte, int>(byteData);

        // endian swap from BE so the math happens in LE space
        BinaryPrimitives.ReverseEndianness(data, data);

        int n = data.Length - 1;
        if (n < 1)
        {
            BinaryPrimitives.ReverseEndianness(data, data);

            return; 
        }

        int p, q = 6 + 52 / (n + 1);

        int z = data[n], y, sum = 0, e;
        while (q-- > 0)
        {
            sum += XXTEA_DELTA;
            e = sum >>> 2 & 3;
            for (p = 0; p < n; p++)
            {
                y = data[p + 1];
                z =
                    data[p] += ((z >>> 5 ^ y << 2) + (y >>> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z));
            }

            y = data[0];
            z =
                data[n] += ((z >>> 5 ^ y << 2) + (y >>> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z));
        }

        // endian swap so the final data is in LE again
        BinaryPrimitives.ReverseEndianness(data, data);
    }

    /// <summary>
    /// In-place decrypts byte data using big endian XXTEA.
    ///
    /// Due to how XXTEA data works, you must pad the data to a multiple of 4 bytes.
    /// </summary>
    /// <param name="byteData">The data to decrypt</param>
    /// <param name="key">The key used to decrypt the data</param>
    /// <exception cref="ArgumentException">The input is not a multiple of 4 bytes</exception>
    /// <remarks>
    /// Referenced from https://github.com/ennuo/toolkit/blob/dc82bee57ab58e9f4bf35993d405529d4cbc7d00/lib/cwlib/src/main/java/cwlib/util/Crypto.java#L97
    /// </remarks>
    public static void XxteaDecrypt(Span<byte> byteData, Span<int> key)
    {
        if (byteData.Length % 4 != 0)
            throw new ArgumentException("Data must be padded to 4 bytes.", nameof(byteData));

        // Alias the byte data as integers
        Span<int> data = MemoryMarshal.Cast<byte, int>(byteData);

        // endian swap from BE so the math happens in LE space
        BinaryPrimitives.ReverseEndianness(data, data);

        int n = data.Length - 1;
        if (n < 1)
        {
            BinaryPrimitives.ReverseEndianness(data, data);

            return; 
        }

        int p, q = 6 + 52 / (n + 1);

        int z, y = data[0], sum = q * XXTEA_DELTA, e;
        while (sum != 0)
        {
            e = sum >>> 2 & 3;
            for (p = n; p > 0; p--)
            {
                z = data[p - 1];
                y = data[p] -=
                    ((z >>> 5 ^ y << 2) + (y >>> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z));
            }

            z = data[n];
            y =
                data[0] -= ((z >>> 5 ^ y << 2) + (y >>> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z));
            sum -= XXTEA_DELTA;
        }

        // endian swap so the final data is in LE again
        BinaryPrimitives.ReverseEndianness(data, data);
    }
}