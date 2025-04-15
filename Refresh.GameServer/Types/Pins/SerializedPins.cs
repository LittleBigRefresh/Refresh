using Bunkum.Core;
using NotEnoughLogs;

namespace Refresh.GameServer.Types.Pins;

#nullable disable
public partial class SerializedPins
{
    /// <summary>
    /// Pins which can have various progress values.
    /// Follows the following pattern: progressType, progress value, progressType, progress value etc.
    /// Can contain the same pins as AwardPins (equal progressType), if it does, the times awarded and progress value
    /// is usually equal per pin (progressType).
    /// </summary>
	[JsonProperty("progress")] public List<long> ProgressPins { get; set; }

    /// <summary>
    /// Pins which can be awarded multiple times.
    /// Follows the following pattern: progressType, times awarded, progressType, times awarded etc.
    /// Can contain the same pins as ProgressPins (equal progressType), if it does, the times awarded and progress value
    /// is usually equal per pin (progressType).
    /// </summary>
	[JsonProperty("awards")] public List<long> AwardPins { get; set; }

    /// <summary>
    /// The progressTypes of pins set to be shown on a user's profile for a certain game, in the order set by the user.
    /// </summary>
	[JsonProperty("profile_pins")] public List<long> ProfilePins { get; set; }

    #nullable enable

    public static Dictionary<long, int> ToDictionary(List<long> rawPins, Logger? logger = null)
    {
        Dictionary<long, int> dictionary = [];

        try
        {
            for (int i = 0; i < rawPins.Count; i += 2)
            {
                long progressType = rawPins[i];
                int progress = (int)rawPins[i + 1];
                logger?.LogDebug(BunkumCategory.UserContent, $"ToDictionary: progressType: {progressType}, progress: {progress}");
                dictionary.Add(progressType, progress);
            }
        }
        // Will likely be thrown if the list's length is odd or if a progress value can't be casted to int.
        // Either way is a bad request and either rare or manipulated.
        catch (Exception ex)
        {
            logger?.LogWarning(BunkumCategory.UserContent, $"Failed to convert pins from list to dictionary: {ex}");
        }
        
        return dictionary;
    }

    /// <summary>
    /// Converts the SerializedPin object's ProgressPins and AwardPins into Dictionaries using <see cref='ToDictionary'/>
    /// and then concatenates them into one Directory, blending out duplicate KeyValuePairs with the same Key (pin's progressType)
    /// and only leaving the ones with the highest Value (pin's progress).
    /// </summary>
    public Dictionary<long, int> ToMergedDictionary(Logger? logger = null)
        => ToDictionary(this.ProgressPins, logger)
            .Concat(ToDictionary(this.AwardPins, logger))
            .GroupBy(p => p.Key)
            .Select(g => new KeyValuePair<long, int> (g.Key, g.Max(p => p.Value)))
            .ToDictionary();
        
    public static SerializedPins FromOld(IEnumerable<PinProgressRelation> pinProgresses, IEnumerable<ProfilePinRelation> profilePins, Logger? logger = null)
    {
        // Convert pin progress relations (both progressTypes and progress values) back to a list
        List<long> rawPinList = [];
        foreach(PinProgressRelation relation in pinProgresses)
        {
            rawPinList.Add(relation.PinId);
            rawPinList.Add(relation.Progress);
        }

        logger?.LogDebug(BunkumCategory.UserContent, $"FromOld: start");

        foreach(long number in rawPinList)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"FromOld: number: {number}");
        }

        return new()
        {
            // Setting both to the same list is easier and has no negative impacts in-game
            ProgressPins = rawPinList,
            AwardPins = rawPinList,
            ProfilePins = profilePins.Select(p => p.PinId).ToList(),
        };
    }
}
