using Refresh.GameServer.Types.Report;

namespace Refresh.GameServer.Database; 

public partial class GameDatabaseContext 
{
    public void AddGriefReport(GameReport report) 
    {
        this.AddSequentialObject(report, () => {});
    }

    public DatabaseList<GameReport> GetGriefReports(int count, int skip)
    {
        return new DatabaseList<GameReport>(this._realm.All<GameReport>(), skip, count);
    }
}