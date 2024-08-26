namespace Refresh.Common.Helpers;

public class ByteArrayComparer : IEqualityComparer<byte[]> {
    public bool Equals(byte[]? a, byte[]? b)
    {
        // If either are null and they arent equal, early return
        if (a == null || b == null && a != b) 
            return false;
        
        if (a.Length != b.Length) 
            return false;

        return a.SequenceEqual(b);
    }
    
    public int GetHashCode(byte[] arr)
    {
        uint hashCode = 0;
        foreach (byte b in arr)
            hashCode = ((hashCode << 23) | (hashCode >> 9)) ^ b;

        return unchecked((int)hashCode);
    }
}