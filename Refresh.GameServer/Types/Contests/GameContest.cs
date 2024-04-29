using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Contests;

#nullable disable

public partial class GameContest : IRealmObject
{
    /// <summary>
    /// The unique identifier for the contest.
    /// </summary>
    /// <remarks>
    /// Must be lowercase and not include special characters.
    /// </remarks>
    /// <example>lbpcc1</example>
    [PrimaryKey] public string ContestId { get; set; }
    /// <summary>
    /// The user responsible for organizing the contest.
    /// </summary>
    /// <remarks>
    /// This will allow this user to modify any aspect of the contest. This does not disallow admins from editing the contest.
    /// </remarks>
    public GameUser Organizer { get; set; }
    
    /// <summary>
    /// When the contest was first created.
    /// </summary>
    public DateTimeOffset CreationDate { get; set; }
    /// <summary>
    /// When the contest will begin
    /// </summary>
    public DateTimeOffset StartDate { get; set; }
    /// <summary>
    /// When the contest will end
    /// </summary>
    public DateTimeOffset EndDate { get; set; }
    
    /// <summary>
    /// The tag or marker to be used to search for levels within the contest.
    /// </summary>
    /// <example>#LBPCC1</example>
    public string ContestTag { get; set; }
    
    /// <summary>
    /// A URL to an image of a banner to be displayed on the website, 'taking over' the front page.
    /// </summary>
    public string BannerUrl { get; set; }
    
    /// <summary>
    /// A friendly name for the contest.
    /// </summary>
    public string ContestTitle { get; set; }
    
    /// <summary>
    /// A one sentence summarization of the contest.
    /// </summary>
    public string ContestSummary { get; set; }
    
    /// <summary>
    /// String containing markdown, describing the contest and its rules in further detail.
    /// </summary>
    public string ContestDetails { get; set; }
    
    public string ContestTheme { get; set; }
    /// <summary>
    /// A URL to an image of the contest theme
    /// </summary>
    public string ContestThemeImageUrl { get; set; }
}