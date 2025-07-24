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
    
    public string LevelHash { get; set; } = "0";
    public string GriefStateHash { get; set; } = "0";
    public ICollection<ReportPlayerRelation> Players { get; set; } = new List<ReportPlayerRelation>();
    
    public int PhotoId { get; set; }
    [ForeignKey("PhotoId")]  
    public GamePhoto Photo { get; set; } = null!;
    public string MarkerRect { get; set; } = string.Empty; // normalized "l,t,r,b" 

    public ReportStatus Status { get; set; } = ReportStatus.Pending; 
    public ReportType Type { get; set; } = ReportType.Unknown;
    public string Description { get; set; } = string.Empty;
    
    public GameUser? ReviewedBy { get; set; }
    public DateTimeOffset? ReviewedDate { get; set; }
    public string? ModeratorNotes { get; set; }
}
