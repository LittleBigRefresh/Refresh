using JetBrains.Annotations;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Reports;

namespace Refresh.Database;

public partial class GameDatabaseContext // Reports
{
    private IQueryable<GriefReport> ReportsIncluded => this.Reports
        .Include(r => r.Reporter)
        .Include(r => r.Reporter.Statistics)
        .Include(r => r.Level)
        .Include(r => r.Level!.Publisher)
        .Include(r => r.Level!.Statistics)
        .Include(r => r.ReviewedBy)
        .Include(r => r.ReviewedBy!.Statistics)
        .Include(r => r.Players)
        .ThenInclude(p => p.User)
        .Include(r => r.Players)
        .ThenInclude(p => p.User.Statistics);
    
    public void CreateReport(GriefReport griefReport)
    {
        this.Write(() =>
        {
            griefReport.ReportDate = this._time.Now;
            this.Reports.Add(griefReport);
        });
    }
    
    public void UpdateReportStatus(GriefReport griefReport, GriefReportStatus status, GameUser? reviewer = null, string? notes = null)
    {
        this.Write(() =>
        {
            griefReport.Status = status;
            griefReport.ReviewedBy = reviewer;
            griefReport.ReviewedDate = reviewer != null ? this._time.Now : null;
            griefReport.ModeratorNotes = notes;
        });
    }
    
    public void RemoveReport(GriefReport griefReport)
    {
        this.Write(() =>
        {
            this.Reports.Remove(griefReport);
        });
    }
    
    [Pure]
    public GriefReport? GetReportById(int id) =>
        this.ReportsIncluded.FirstOrDefault(r => r.ReportId == id);
    
    [Pure]
    public DatabaseList<GriefReport> GetReportsByStatus(GriefReportStatus status, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GriefReport> GetReportsByReporter(GameUser reporter, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Reporter == reporter)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GriefReport> GetReportsForLevel(GameLevel level, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Level == level)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GriefReport> GetStaleReports(TimeSpan maxAge, int count, int skip)
    {
        DateTimeOffset cutoff = this._time.Now - maxAge;
        return new(this.ReportsIncluded
            .Where(r => r.Status == GriefReportStatus.Pending && r.ReportDate < cutoff)
            .OrderBy(r => r.ReportDate), skip, count);
    }

}