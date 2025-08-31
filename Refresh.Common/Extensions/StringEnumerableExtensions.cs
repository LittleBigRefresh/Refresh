using System.Text;

namespace Refresh.Common.Extensions;

public static class StringEnumerableExtensions
{
    public static string ToCommaSeperatedList(this IEnumerable<string> strings)
    {
        StringBuilder allStrings = new();
        
        foreach (string str in strings)
        {
            allStrings.Append(str);
            allStrings.Append(',');
        }

        if (allStrings.Length > 1)
            allStrings.Remove(allStrings.Length - 1, 1);

        return allStrings.ToString();
    }
}
