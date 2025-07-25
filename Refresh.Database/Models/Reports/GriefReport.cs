using System.Drawing;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Reports; 

public class GriefReport 
{
    [Key] public int ReportId { get; set; }
    
    public GameUser Reporter { get; set; } = null!;
    public DateTimeOffset ReportDate { get; set; }
    public TokenGame GameVersion { get; set; }
    
    // Level data
    public int? LevelId { get; set; }
    [ForeignKey("LevelId")]
    public GameLevel? Level { get; set; } // Level reference only provided if it's an uploaded or story level
    public string? LevelType { get; set; }
    public string InitialStateHash { get; set; } = "0"; // Hash for asset of initial level
    public string GriefStateHash { get; set; } = "0"; // Hash for asset of griefed level
    
    // Evidence
    public string PhotoAssetHash { get; set; } = "0";
    public string MarkerRect { get; set; } = string.Empty; // Normalized Rect (-1,1) "l,t,r,b" 
    public ICollection<ReportPlayerRelation> Players { get; set; } = new List<ReportPlayerRelation>();
    
    // Report details
    public GriefReportType Type { get; set; } = GriefReportType.Unknown;
    public string Description { get; set; } = string.Empty;
    
    // Moderation
    public GameUser? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedDate { get; set; }
    public string? ModeratorNotes { get; set; }
    public GriefReportStatus Status { get; set; } = GriefReportStatus.Pending; 

}