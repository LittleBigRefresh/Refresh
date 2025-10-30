using System.Text;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database.Models.Levels.Challenges;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub.Ghost;

[XmlRoot("ghost")]
[XmlType("ghost")]
public class SerializedChallengeGhost
{
    /// <summary>
    /// Checkpoints activated during the challenge.
    /// </summary>
    [XmlElement("checkpoint")] public List<SerializedChallengeCheckpoint> Checkpoints { get; set; } = [];
    /// <summary>
    /// Tracked player movement during the challenge. We only read them to make sure they exist.
    /// </summary>
    [XmlElement("ghost_frame")] public List<SerializedChallengeGhostFrame> Frames { get; set; } = [];

    /// <summary>
    /// Returns the ghost asset specified by the given hash from the given data store as a SerializedChallengeGhost.
    /// Returns null if it fails to do so.
    /// This method assumes that if there is an asset found under that hash, it is a ChallengeGhost.
    /// </summary>
    /// <param name="logger">Optional logger, which if passed, logs potential exceptions while the method actually deserializes
    /// the ghost asset found in the given IDataStore</param>
    public static SerializedChallengeGhost? FromDataStore(string ghostHash, IDataStore dataStore, Logger? logger = null)
    {
        // Try to get the ghost asset's contents as a string
        if (!dataStore.TryGetDataFromStore(ghostHash, out byte[]? assetBytes) || assetBytes == null)
        {
            logger?.LogWarning(BunkumCategory.UserContent, $"Ghost asset {ghostHash} couldn't be read!");
            return null;
        }

        string assetString = Encoding.ASCII.GetString(assetBytes);

        // Check "magic"
        if (assetString.Length < 7 || !assetString.StartsWith("<ghost>"))
        {
            logger?.LogWarning(BunkumCategory.UserContent, $"Ghost asset {ghostHash} is not a ghost asset!");
            return null;
        }

        // Since the "metric" elements usually contain duplicate "id" attributes, which is invalid XML,
        // and we don't even need to read them for anything, remove them by creating a new string containing 
        // everything but the metric elements
        string[] assetSubstrings = assetString.Split(["<metric", "</metric>"], StringSplitOptions.None);
        string fixedAssetString = "";
        for (int i = 0; i < assetSubstrings.Length; i += 2) 
        {
            fixedAssetString += assetSubstrings[i];
        } 

        // Try to deserialize the ghost asset
        SerializedChallengeGhost? serializedGhost = null;
        try
        {
            XmlSerializer ghostSerializer = new(typeof(SerializedChallengeGhost));
            if (ghostSerializer.Deserialize(new StringReader(fixedAssetString)) is not SerializedChallengeGhost output)
            {
                logger?.LogWarning(BunkumCategory.UserContent, $"Deserialized ghost asset {ghostHash} is not a SerializedChallengeGhost!");
                return null;
            }

            serializedGhost ??= output;
        }
        catch (Exception ex)
        {
            // If a logger is passed to this method, log the exception
            logger?.LogWarning(BunkumCategory.UserContent, $"Error deserializing ghost asset: {ex}");
            return null;
        }

        return serializedGhost;
    }

    /// <summary>
    /// Does some simple checks on the given SerializedChallengeGhost and returns whether they were successful or not.
    /// </summary>
    /// <param name="isFirstScore">Whether the given SerializedChallengeGhost's score is the first one submitted to the given challenge.</param>
    // There does not seem to be a way to catch all kinds of corruptions possible by LBP hub, neither is there a reliable way to 
    // correct corrupt ghost data either, so just try to do some easy checks on the given SerializedChallengeGhost.
    public static bool IsGhostDataValid(SerializedChallengeGhost challengeGhost, GameChallenge challenge, bool isFirstScore, Logger? logger = null)
    {
        if (challengeGhost.Checkpoints.Count < 1)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"No checkpoints, rejecting ghost asset");
            return false;
        }

        if (challengeGhost.Frames.Count < 1)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"No frames, rejecting ghost asset");
            return false;
        }

        // Normally the game already takes care of this, but just in case
        IEnumerable<SerializedChallengeCheckpoint> checkpoints = challengeGhost.Checkpoints.OrderBy(c => c.Time);

        if (checkpoints.First().Uid != challenge.StartCheckpointUid || checkpoints.Last().Uid != challenge.FinishCheckpointUid)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"First or last checkpoint doesn't match up with challenge data, rejecting ghost asset");
            return false;
        }

        // The finish checkpoint can't appear more than once in a score which is not the first score, 
        // because the game immediately ends the challenge you are playing when it gets activated
        if (!isFirstScore && checkpoints.Count(c => c.Uid == challenge.FinishCheckpointUid) > 1)
        {
            logger?.LogDebug(BunkumCategory.UserContent, $"Multiple finish checkpoints when not possible, rejecting ghost asset");
            return false;
        }

        return true;
    }
}