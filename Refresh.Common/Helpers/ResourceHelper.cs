using System.Buffers;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection;
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
}