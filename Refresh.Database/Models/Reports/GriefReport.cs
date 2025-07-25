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
    public TokenGame GameVersion;

    
    // level/evidence data
    public int? LevelId { get; set; }
    [ForeignKey("LevelId")]
    public GameLevel? Level { get; set; }
    public string LevelType { get; set; } = string.Empty;
    public string InitialStateHash { get; set; } = "0"; // Hash for asset of initial level
    public string GriefStateHash { get; set; } = "0"; // Hash for asset of griefed level
    
    // evidence
    public string PhotoAssetHash { get; set; } = "0";
    public string MarkerRect { get; set; } = string.Empty; // normalized "l,t,r,b" 
    public ICollection<ReportPlayerRelation> Players { get; set; } = new List<ReportPlayerRelation>();
    
    // report details
    public GriefReportType Type { get; set; } = GriefReportType.Unknown;
    public string Description { get; set; } = string.Empty;
    public GriefReportStatus Status { get; set; } = GriefReportStatus.Pending; 
    
    // Moderation
    public GameUser? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedDate { get; set; }
    public string? ModeratorNotes { get; set; }
}