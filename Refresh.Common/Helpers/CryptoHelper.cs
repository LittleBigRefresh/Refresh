using System.Security.Cryptography;

namespace Refresh.Common.Helpers;

public static class CryptoHelper
{
    public static string GetRandomBase64String(int length)
    {
        byte[] tokenData = new byte[length];
        
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenData);

        return Convert.ToBase64String(tokenData);
    }
}