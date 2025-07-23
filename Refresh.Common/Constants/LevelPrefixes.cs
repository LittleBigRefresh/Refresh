using System.Text.RegularExpressions;

namespace Refresh.Common.Constants;

public static partial class LevelPrefixes
{
    
    /// <summary>
    /// Matches values of ?key:value or ?key.value
    /// </summary>
    [GeneratedRegex(@"\?(\w+)[\.:]([\w-]+)")]
    private static partial Regex AttributeRegex();
    
    /// <summary>
    /// Extracts matches from AttributeRegex into a dictionary 
    /// </summary>
    public static Dictionary<string, string> ExtractAttributes(string description)
    {
        MatchCollection matches = AttributeRegex().Matches(description);
        return matches.ToDictionary(
            m => m.Groups[1].Value, 
            m => m.Groups[2].Value
        );
    }
    
    /// <summary>
    /// Keywords that indicate a level is a reupload
    /// </summary>
    public static readonly string[] ReuploadKeywords = { 
        "(reupload)", "[reupload]",
        "(reuploaded)", "[reuploaded]",
        "(archive)", "[archive]",
        "(archived)", "[archived]",
        "(republish)", "[republish]",
        "(republished)", "[republished]",
    };
}