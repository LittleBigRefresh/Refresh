using System.Text;
using System.Xml.Serialization;
using Bunkum.Core.Storage;

namespace Refresh.GameServer.Types.Challenges.LbpHub.Ghost;

[XmlRoot("ghost")]
[XmlType("ghost")]
public class SerializedChallengeGhost
{
    /// <summary>
    /// Checkpoints activated during the challenge.
    /// </summary>
    [XmlElement("checkpoint")] public List<SerializedChallengeCheckpoint> Checkpoints { get; set; } = [];
    /// <summary>
    /// Tracked player movement during the challenge. Not needed for now.
    /// </summary>
    // [XmlElement("ghost_frame")] public List<SerializedChallengeGhostFrame> Frames { get; set; } = [];

    /// <summary>
    /// Returns the ghost asset specified by the given hash from the given data store as a SerializedChallengeGhost.
    /// Returns null if it fails to do so.
    /// This method assumes that if there is an asset found under that hash, it is a ChallengeGhost.
    /// </summary>
    public static SerializedChallengeGhost? GetSerializedChallengeGhostFromDataStore(string? ghostHash, IDataStore dataStore)
    {
        if (ghostHash == null)
            return null;

        // try to get the ghost asset's contents as a string
        if (!dataStore.TryGetDataFromStore(ghostHash, out byte[]? ghostContentBytes) || ghostContentBytes == null)
            return null;

        string ghostContentString = Encoding.ASCII.GetString(ghostContentBytes);

        // Since the "metric" elements usually contain duplicate "id" attributes (preventing the xml serializer from deserializing our asset)
        // and we don't even need them for anything (outside of storing them), remove them by
        // first replacing all opening and closing metric tags with an invalid character...
        ghostContentString = ghostContentString.Replace("<metric", "&").Replace("</metric>", "&");

        // ...and then creating a new string containing everything but the metric elements
        string[] ghostContentSubstrings = ghostContentString.Split('&');
        string fixedGhostContentString = "";
        for (int i = 0; i < ghostContentSubstrings.Length; i += 2) 
        {
            fixedGhostContentString += ghostContentSubstrings[i];
        } 

        // Try to deserialize the ghost asset
        SerializedChallengeGhost? serializedGhost = null;
        try
        {
            XmlSerializer ghostSerializer = new(typeof(SerializedChallengeGhost));
            if (ghostSerializer.Deserialize(new StringReader(fixedGhostContentString)) is not SerializedChallengeGhost output)
                return null;

            serializedGhost ??= output;
        }
        catch
        {
            return null;
        }

        return serializedGhost;
    }

    /// <summary>
    /// Does some simple checks on the given SerializedChallengeGhost and returns whether they were successful or not.
    /// </summary>
    // There does not seem to be a way to catch all kinds of corruptions possible by LBP hub, neither is there a reliable way to 
    // correct corrupt ghost data either, so just try to do some easy checks on the given SerializedChallengeGhost.
    public static bool IsGhostDataValid(SerializedChallengeGhost? challengeGhost, GameChallenge challenge, bool isFirstScore)
    {
        if (challengeGhost == null || challengeGhost.Checkpoints.Count < 1)
            return false;

        // Normally the game already takes care of this, but just in case
        IEnumerable<SerializedChallengeCheckpoint> checkpoints = challengeGhost.Checkpoints.OrderBy(c => c.Time);

        if (checkpoints.First().Uid != challenge.StartCheckpointUid || checkpoints.Last().Uid != challenge.FinishCheckpointUid)
            return false;

        // The finish checkpoint cant appear more than once in a score which is not the first score, 
        // because the game immediately ends the challenge you are playing when it gets activated
        if (!isFirstScore && checkpoints.Count(c => c.Uid == challenge.FinishCheckpointUid) > 1)
            return false;

        return true;
    }
}