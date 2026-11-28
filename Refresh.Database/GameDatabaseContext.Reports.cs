using JetBrains.Annotations;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Reports;

namespace Refresh.Database;

public partial class GameDatabaseContext // Reports
{
    private IQueryable<GameGriefReport> GameGriefReportsIncluded => this.GameGriefReports
        .Include(r => r.Reporter)
        .Include(r => r.Reporter.Statistics)
        .Include(r => r.Level)
        .Include(r => r.Level!.Publisher)
        .Include(r => r.Level!.Statistics)
        .Include(r => r.ReviewedBy)
        .Include(r => r.ReviewedBy!.Statistics)
        .Include(r => r.Players)
        .ThenInclude(p => p.User!)
        .ThenInclude(p => p.Statistics);
    
    public void CreateReport(GameGriefReport griefReport)
    {
        this.Write(() =>
        {
            griefReport.ReportDate = this._time.Now;
            this.GameGriefReports.Add(griefReport);
        });
    }
    
    public void UpdateReportStatus(GameGriefReport griefReport, GriefReportStatus status, GameUser? reviewer = null, string? notes = null)
    {
        this.Write(() =>
        {
            griefReport.Status = status;
            griefReport.ReviewedBy = reviewer;
            griefReport.ReviewedDate = reviewer != null ? this._time.Now : null;
            griefReport.ModeratorNotes = notes;
        });
    }
    
    public void RemoveReport(GameGriefReport griefReport)
    {
        this.Write(() =>
        {
            this.GameGriefReports.Remove(griefReport);
        });
    }
    
    [Pure]
    public GameGriefReport? GetReportById(int id) =>
        this.GameGriefReportsIncluded.FirstOrDefault(r => r.ReportId == id);
    
    [Pure]
    public DatabaseList<GameGriefReport> GetReportsByStatus(GriefReportStatus status, int count, int skip) =>
        new(this.GameGriefReportsIncluded
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GameGriefReport> GetReportsByReporter(GameUser reporter, int count, int skip) =>
        new(this.GameGriefReportsIncluded
            .Where(r => r.ReporterUserId == reporter.UserId)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GameGriefReport> GetReportsForLevel(GameLevel level, int count, int skip) =>
        new(this.GameGriefReportsIncluded
            .Where(r => r.LevelId == level.LevelId)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<GameGriefReport> GetStaleReports(TimeSpan maxAge, int count, int skip)
    {
        DateTimeOffset cutoff = this._time.Now - maxAge;
        return new(this.GameGriefReportsIncluded
            .Where(r => r.Status == GriefReportStatus.Pending && r.ReportDate < cutoff)
            .OrderBy(r => r.ReportDate), skip, count);
    }

}