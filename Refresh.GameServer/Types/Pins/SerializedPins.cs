using Bunkum.Core;
using NotEnoughLogs;
using Refresh.GameServer.Types.Data;

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

    /// <remarks>
    /// Can throw if either the rawPins list has an odd length or if a progress value from that list can't be casted to long.
    /// Either would be a bad request.
    /// </remarks>
    public static Dictionary<long, int> ToDictionary(List<long> rawPins)
    {
        Dictionary<long, int> dictionary = [];

        for (int i = 0; i < rawPins.Count; i += 2)
        {
            long progressType = rawPins[i];
            int progress = (int)rawPins[i + 1];
                
            dictionary.Add(progressType, progress);
        }
        
        return dictionary;
    }

    /// <summary>
    /// Converts ProgressPins and AwardPins to Dictionaries (progressType as Key and progress value as Value) by using
    /// <see cref="ToDictionary"/> and then merges them to one Dictionary, taking KeyValuePairs with the highest progress values
    /// for pairs with duplicate progressTypes.
    /// </summary>
    /// <remarks>
    /// Since this method uses <see cref="ToDictionary"/>, it can throw for the same reasons as ToDictionary.
    /// </remarks>
    public Dictionary<long, int> ToMergedDictionary()
        => ToDictionary(this.ProgressPins)
            .Concat(ToDictionary(this.AwardPins))
            .GroupBy(p => p.Key)
            .Select(g => new KeyValuePair<long, int> (g.Key, g.Max(p => p.Value)))
            .ToDictionary();
        
    public static SerializedPins FromOld(IEnumerable<PinProgressRelation> pinProgresses, IEnumerable<ProfilePinRelation> profilePins)
    {
        // Convert pin progress relations (both progressTypes and progress values) back to a list
        List<long> rawPinList = [];
        foreach(PinProgressRelation relation in pinProgresses)
        {
            rawPinList.Add(relation.PinId);
            rawPinList.Add(relation.Progress);
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
