using JetBrains.Annotations;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Reports;

namespace Refresh.Database;

public partial class GameDatabaseContext // Reports
{
    private IQueryable<Report> ReportsIncluded => this.Reports
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
    
    public void CreateReport(Report report)
    {
        this.Write(() =>
        {
            report.ReportDate = this._time.Now;
            this.Reports.Add(report);
        });
    }
    
    public void UpdateReportStatus(Report report, ReportStatus status, GameUser? reviewer = null, string? notes = null)
    {
        this.Write(() =>
        {
            report.Status = status;
            report.ReviewedBy = reviewer;
            report.ReviewedDate = reviewer != null ? this._time.Now : null;
            report.ModeratorNotes = notes;
        });
    }
    
    public void RemoveReport(Report report)
    {
        this.Write(() =>
        {
            this.Reports.Remove(report);
        });
    }
    
    [Pure]
    public Report? GetReportById(int id) =>
        this.ReportsIncluded.FirstOrDefault(r => r.ReportId == id);
    
    [Pure]
    public DatabaseList<Report> GetReportsByStatus(ReportStatus status, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<Report> GetReportsByReporter(GameUser reporter, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Reporter == reporter)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<Report> GetReportsForLevel(GameLevel level, int count, int skip) =>
        new(this.ReportsIncluded
            .Where(r => r.Level == level)
            .OrderByDescending(r => r.ReportDate), skip, count);
    
    [Pure]
    public DatabaseList<Report> GetStaleReports(TimeSpan maxAge, int count, int skip)
    {
        DateTimeOffset cutoff = this._time.Now - maxAge;
        return new(this.ReportsIncluded
            .Where(r => r.Status == ReportStatus.Pending && r.ReportDate < cutoff)
            .OrderBy(r => r.ReportDate), skip, count);
    }

}