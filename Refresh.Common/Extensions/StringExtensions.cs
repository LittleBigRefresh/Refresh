namespace Refresh.Common.Extensions;

public static class StringExtensions
{
    public static bool IsHashBlank(this string? hash)
    {
        return string.IsNullOrWhiteSpace(hash) || hash == "0";
    }
}