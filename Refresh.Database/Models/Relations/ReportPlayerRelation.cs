using MongoDB.Bson;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

[PrimaryKey(nameof(ReportId), nameof(UserId))]
public partial class ReportPlayerRelation
{
    [Required]
    [ForeignKey(nameof(ReportId))]
    public GameGriefReport GameGriefReport { get; set; } = null!;
    [Required] public int ReportId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public GameUser? User { get; set; }
    public ObjectId? UserId { get; set; }
    
    public bool IsReporter { get; set; }
    public bool IsInGameNow { get; set; } // i think something about whether the player left during the report, could rename it if confirmed
    public int PlayerNumber { get; set; }
    
    // TODO this and photo subject bounds should be stored in a more atomized way
    public string PlayerRect { get; set; } = string.Empty; // normalized "l,t,r,b"
    
    public DateTimeOffset Timestamp { get; set; }
}