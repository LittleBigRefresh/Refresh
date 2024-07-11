using System.Text;

namespace Refresh.Common.Extensions;

public static class MemoryStreamExtensions
{
    public static void WriteString(this MemoryStream ms, string str) => ms.Write(Encoding.UTF8.GetBytes(str));
}