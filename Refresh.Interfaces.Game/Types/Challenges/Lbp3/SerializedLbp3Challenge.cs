using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Challenges.Lbp3;

#nullable disable

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
    /// The progress type of the pin indicating whether the user's highscore is bronze, silver or gold (progress value is 1-3 accordingly)
    /// </summary>
    [XmlAttribute("Challenge_PinId")]
    public ulong ScoreMedalPinProgressType { get; set; }
    
    /// <summary>
    /// The progress type of the Pin indicating whether the user is in the top 50%, 25% or 10% of the leaderboard
    /// </summary>
    // TODO: As soon as score submission for adventures/LBP3 challenges is implemented,
    // find out whether these need to be awarded by the server, and also find out
    // whether these pins have a descending progress and special case them
    // in the progress update method if they do.
    [XmlAttribute("Challenge_RankPin")]
    public ulong ScoreRankingPinProgressType { get; set; }
    
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