using System.Collections.Frozen;

namespace Refresh.Common.Helpers;

public static class VanillaHashChecker
{
    // A hashset of all the vanilla hashes.
    // This would be a FrozenSet<string> since we use strings for hashes everywhere else,
    // but the perf was like 2x worse and it took 4x the memory,
    // even with the overhead of the string -> byte[] conversion, each check is cheaper this way.
    private static readonly FrozenSet<byte[]> VanillaHashes;

    static VanillaHashChecker()
    {
        HashSet<byte[]> set = new(new ByteArrayComparer());
        
        using Stream stream = ResourceHelper.StreamFromResource("Refresh.Common.Resources.all_hashes.txt");
        
        StreamReader reader = new(stream);

        //Read all the lines
        while (reader.ReadLine() is {} line)
        {
            //Parse out and add each line to the set
            set.Add(HexHelper.HexStringToBytes(line));
        }

        VanillaHashes = set.ToFrozenSet(new ByteArrayComparer());
    }
    
    public static bool IsVanillaHash(string hash)
    {
        byte[] hashArr = HexHelper.HexStringToBytes(hash);

        return VanillaHashes.Contains(hashArr);
    }
}