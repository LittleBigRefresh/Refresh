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
    
    // https://manuals.playstation.net/document/en/store/signup.html
    // https://github.com/RipleyTom/rpcn/blob/master/src/server/client/cmd_account.rs#L209 (NpId check of pub fn create_account)
    // we removed the first letter restriction from sony's official restrictions to match RPCN
    [GeneratedRegex("^[a-zA-Z0-9_-]{3,16}$")]
    public static partial Regex UsernameRegex();
}