using System.Drawing;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Reports; 

public class Report 
{
    [Key] public int ReportId { get; set; }
    
    public GameUser Reporter { get; set; } = null!;
    public DateTimeOffset ReportDate { get; set; }
    
    // level/evidence data
    public int? LevelId { get; set; }
    [ForeignKey("LevelId")]
    public GameLevel? Level { get; set; }
    public string LevelType { get; set; } = string.Empty;
    public string InitialStateHash { get; set; } = "0"; // hash for initial level state asset
    public string GriefStateHash { get; set; } = "0"; // hash for griefed level state asset
    
    // evidence
    public string PhotoAssetHash { get; set; } = "0";
    public string MarkerRect { get; set; } = string.Empty; // normalized "l,t,r,b" 
    public ICollection<ReportPlayerRelation> Players { get; set; } = new List<ReportPlayerRelation>();
    
    // report details
    public ReportType Type { get; set; } = ReportType.Unknown;
    public string Description { get; set; } = string.Empty;
    public ReportStatus Status { get; set; } = ReportStatus.Pending; 
    
    // Moderation
    public GameUser? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedDate { get; set; }
    public string? ModeratorNotes { get; set; }
}