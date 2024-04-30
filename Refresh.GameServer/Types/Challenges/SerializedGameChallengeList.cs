using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges;

[XmlRoot("Challenge_header")]
public class SerializedGameChallengeList
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
    public List<SerializedGameChallenge> Challenges { get; set; }
}

public class SerializedGameChallenge
{
    /// <summary>
    /// A sequential ID from 0 of which challenge this is
    /// </summary>
    [XmlAttribute("Challenge_ID")]
    public int Id { get; set; }
    
    /// <summary>
    /// A unix epoch timestamp in microseconds for when this challenge starts
    /// </summary>
    [XmlAttribute("Challenge_active_date_starts")]
    public ulong StartTime { get; set; }
    
    /// <summary>
    /// A unix epoct timestamp in microseconds for when this challenge ends
    /// </summary>
    [XmlAttribute("Challenge_active_date_ends")]
    public ulong EndTime { get; set; }
    
    /// <summary>
    /// The LAMS description ID for this challenge's description
    /// </summary>
    [XmlAttribute("Challenge_LAMSDescription_Id")]
    public string LamsDescriptionId { get; set; }
    
    /// <summary>
    /// The LAMS description ID for this challenge's title
    /// </summary>
    [XmlAttribute("Challenge_LAMSTitle_Id")]
    public string LamsTitleId { get; set; }
    
    /// <summary>
    /// The ID of the pin you receive for completing this challenge
    /// </summary>
    [XmlAttribute("Challenge_PinId")]
    public ulong PinId { get; set; }
    
    [XmlAttribute("Challenge_RankPin")]
    public ulong RankPin { get; set; }
    
    /// <summary>
    /// A PSN DLC id for the DLC associated with this challenge
    /// </summary>
    [XmlAttribute("Challenge_Content")]
    public string Content { get; set; }
    
    /// <summary>
    /// The LAMS translation ID for this challenge's content
    /// </summary>
    [XmlAttribute("Challenge_Content_name")]
    public string ContentName { get; set; }
    
    [XmlAttribute("Challenge_Planet_User")]
    public string PlanetUser { get; set; }
    
    [XmlAttribute("Challenge_planetId")]
    public ulong PlanetId { get; set; }
    
    [XmlAttribute("Challenge_photo_1")]
    public ulong PhotoId { get; set; }
}