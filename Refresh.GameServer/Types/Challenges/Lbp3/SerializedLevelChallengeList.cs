using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.Lbp3;

#nullable disable

[XmlRoot("Challenge_header")]
public class SerializedLbp3ChallengeList
{
    [XmlElement("Total_challenges")]
    public int TotalChallenges { get; set; }
    
    /// <summary>
    /// Timestamp is stored as a unix epoch in microseconds, is equal to the very last challenge's end date
    /// </summary>
    [XmlElement("Challenge_End_Date")]
    public ulong EndTime { get; set; }
    
    /// <summary>
    /// Percentage required to get bronze, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Bronze_Range")]
    public float BronzeRankPercentage { get; set; }
    
    /// <summary>
    /// Percentage required to get silver, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Silver_Range")]
    public float SilverRankPercentage { get; set; }
    
    /// <summary>
    /// Percentage required to get gold, stored as a float 0-1
    /// </summary>
    [XmlElement("Challenge_Top_Rank_Gold_Range")]
    public float GoldRankPercentage { get; set; }
    
    /// <summary>
    /// Cycle time stored as a unix epoch in microseconds
    /// </summary>
    [XmlElement("Challenge_CycleTime")]
    public ulong CycleTime { get; set; }
    
    // ReSharper disable once IdentifierTypo
    [XmlElement("item_data")]
    public List<SerializedLbp3Challenge> Challenges { get; set; }
}