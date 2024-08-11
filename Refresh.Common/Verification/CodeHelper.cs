using System.Security.Cryptography;

namespace Refresh.Common.Verification;

public static class CodeHelper
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    
    public static string GenerateDigitCode()
    {
        ReadOnlySpan<byte> validChars = "0123456789"u8;
        Span<char> result = stackalloc char[6];
        Span<byte> randomBytes = stackalloc byte[6];

        Rng.GetBytes(randomBytes);
            
        for (int i = 0; i < randomBytes.Length; i++)
        {
            int index = randomBytes[i] % validChars.Length;
            result[i] = (char)validChars[index];
        }

        return new string(result);
    }
}