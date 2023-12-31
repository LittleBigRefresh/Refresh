using System.Buffers;
using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using FastAes;
using IronCompress;

namespace Refresh.GameServer.Resources;

public static class ResourceHelper
{
    public static Stream StreamFromResource(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(name)!;
    }
    
    private static readonly ThreadLocal<Iron> Iron = new(() => new Iron(ArrayPool<byte>.Shared));

    private static void Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
    {
        throw new NotImplementedException(); //TODO
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