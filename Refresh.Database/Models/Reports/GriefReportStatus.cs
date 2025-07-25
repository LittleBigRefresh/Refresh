namespace Refresh.Database.Models.Reports; 

public enum GriefReportStatus
{
    Pending = 0,        // Awaiting review
    InReview = 1,       // Being actively investigated by a moderator
    Resolved = 2,       // Action taken
    Dismissed = 3,      // Reviewed but no action needed
    Duplicate = 4,      // Same issue already reported
    AutoResolved = 5,   // Automatically handled by system (e.g., user already banned)
    Escalated = 6       // Needs higher attention
}