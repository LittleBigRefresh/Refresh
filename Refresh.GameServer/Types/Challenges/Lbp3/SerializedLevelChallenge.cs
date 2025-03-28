using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.Lbp3;

public class SerializedLbp3Challenge
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