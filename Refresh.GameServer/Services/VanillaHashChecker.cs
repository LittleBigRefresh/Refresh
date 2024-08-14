using Refresh.Common.Helpers;
using Refresh.GameServer.Resources;

namespace Refresh.GameServer.Services;

public static class VanillaHashChecker
{
    // A hashset of all the vanilla hashes.
    // This would be a HashSet<string> since we use strings for hashes everywhere else,
    // but the perf was like 2x worse and it took 4x the memory,
    // even with the overhead of the string -> byte[] conversion, each check is cheaper this way.
    private static readonly HashSet<byte[]> VanillaHashes = new(new ByteArrayComparer());

    static VanillaHashChecker()
    {
        using Stream stream = ResourceHelper.StreamFromResource("Refresh.GameServer.Resources.all_hashes.txt");
        
        StreamReader reader = new(stream);

        //Read all the lines
        while (reader.ReadLine() is {} line)
        {
            //Parse out and add each line to the set
            VanillaHashes.Add(HexHelper.HexStringToBytes(line));
        }
    }
    
    public static bool IsVanillaHash(string hash)
    {
        byte[] hashArr = HexHelper.HexStringToBytes(hash);

        return VanillaHashes.Contains(hashArr);
    }
}