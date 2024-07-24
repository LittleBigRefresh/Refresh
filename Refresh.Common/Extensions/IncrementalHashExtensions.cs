using System.Security.Cryptography;
using System.Text;

namespace Refresh.Common.Extensions;

public static class IncrementalHashExtensions
{
    public static void WriteString(this IncrementalHash hash, string str) => hash.AppendData(Encoding.UTF8.GetBytes(str));
}