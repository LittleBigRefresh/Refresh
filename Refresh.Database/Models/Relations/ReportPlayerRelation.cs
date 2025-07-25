using MongoDB.Bson;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

[PrimaryKey(nameof(ReportId), nameof(UserId))]
public partial class ReportPlayerRelation
{
    [Required]
    [ForeignKey(nameof(ReportId))]
    public GriefReport GriefReport { get; set; }
    
    [Required]
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    [Required] public int ReportId { get; set; }
    [Required] public ObjectId UserId { get; set; }
    
    public bool IsReporter { get; set; }
    public bool IsInGameNow { get; set; } // i think something about whether the player left during the report, could rename it if confirmed
    public int PlayerNumber { get; set; }
    
    public string PlayerRect { get; set; } = string.Empty; // normalized "l,t,r,b"
    
    public DateTimeOffset Timestamp { get; set; }
}