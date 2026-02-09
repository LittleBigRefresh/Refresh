namespace Refresh.Common.Extensions;

public static class StringExtensions
{
    public static bool IsBlankHash(this string? hash)
    {
        return string.IsNullOrWhiteSpace(hash) || hash == "0";
    }
}