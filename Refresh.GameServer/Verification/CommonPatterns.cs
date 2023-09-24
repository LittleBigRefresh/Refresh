using System.Text.RegularExpressions;

namespace Refresh.GameServer.Verification;

public static partial class CommonPatterns
{
    [GeneratedRegex("^[a-f0-9]{40}$")]
    public static partial Regex Sha1Regex();
    
    [GeneratedRegex("^[a-f0-9]{128}$")]
    public static partial Regex Sha512Regex();
    
    [GeneratedRegex("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+[.][a-zA-Z]{2,}$")]
    public static partial Regex EmailAddressRegex();
}