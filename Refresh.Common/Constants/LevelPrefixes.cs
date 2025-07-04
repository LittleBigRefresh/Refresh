using System.Text.RegularExpressions;

namespace Refresh.Common.Constants;

public static partial class LevelPrefixes
{
    [GeneratedRegex(@"\?(\w+)\.([\w-]+)")]
    public static partial Regex AttributeRegex();
    
    /// <summary>
    /// Keywords that indicate a level is a reupload
    /// </summary>
    public static readonly string[] ReuploadKeywords = { 
        "(reupload)", "[reupload]",
        "(archive)", "[archive]"
    };
}