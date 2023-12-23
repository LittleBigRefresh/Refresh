using System.Numerics;
using System.Reflection;
using Microsoft.Toolkit.HighPerformance;

namespace Refresh.GameServer.Resources;

public static class ResourceHelper
{
    public static Stream StreamFromResource(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(name)!;
    }

    private static unsafe void XorVectorized(Span<byte> data, ReadOnlySpan<byte> key)
    {
        fixed (byte* dataPtr = data)
        {
            //Get the misalignment of the data
            int misalignment = (int)((nint)dataPtr % sizeof(nint));

            if (misalignment != 0)
            {
                //XOR the data before the aligned data
                XorScalar(data[..misalignment], key);
            }

            //Get the span of data which is aligned to word boundaries
            Span<byte> alignedData = data[misalignment..];
                
            //Get a Vector<byte> of the data, offset by the misalignment, making this data byte-aligned
            Span<Vector<byte>> alignedDataVec = alignedData.Cast<byte, Vector<byte>>();
            //Get a Vector<byte> of the key, offset by the misalignment
            ReadOnlySpan<Vector<byte>> keyVec = key[misalignment..].Cast<byte, Vector<byte>>();

            //Iterate over every item in the data vector
            for (int i = 0; i < alignedDataVec.Length; i++) 
                //XOR it with the key
                alignedDataVec[i] ^= keyVec[i];

            //Get the amount of bytes which are left over
            int leftoverCount = data.Length - (alignedDataVec.Length * Vector<byte>.Count * sizeof(byte)) - misalignment;
                
            //If theres no leftover bytes, return out
            if (leftoverCount <= 0) return;
                
            //Get the start index of the leftover bytes
            int leftoverOffset = data.Length - leftoverCount;
            //Slice the data to only have the leftover data
            Span<byte> leftover = data[leftoverOffset..];

            XorScalar(leftover, key[leftoverOffset..]);
        }
    }

    private static void XorScalar(Span<byte> data, ReadOnlySpan<byte> key)
    {
        //Iterate over all bytes
        for (int i = 0; i < data.Length; i++) 
            //Iterate over all data, and XOR it with the key
            data[i] ^= key[i];
    }
    
    public static unsafe void Xor(Span<byte> data, ReadOnlySpan<byte> key)
    {
        if (key.Length < data.Length) throw new ArgumentException("Key must be as long or longer than the data!", nameof(key));
        
        if (Vector<byte>.IsSupported && data.Length >= Vector<byte>.Count * sizeof(byte))
            XorVectorized(data, key);
        else
            XorScalar(data, key);
    }
}