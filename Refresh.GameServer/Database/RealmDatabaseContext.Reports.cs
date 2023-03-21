using Refresh.GameServer.Types.Report;

namespace Refresh.GameServer.Database; 

public partial class RealmDatabaseContext 
{
    public void AddGriefReport(GriefReport report) 
    {
        this.AddSequentialObject(report, () => {});
    }    
}