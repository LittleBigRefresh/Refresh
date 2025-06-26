using Refresh.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.Workers.Workers;

public class ObjectStatisticsWorker : IWorker
{
    public int WorkInterval => 10_000;
    public void DoWork(DataContext context)
    {
        GameLevel[] levels = context.Database.GetLevelsWithStatisticsNeedingUpdates().ToArray();

        foreach (GameLevel level in levels)
        {
            context.Database.RecalculateLevelStatistics(level);
        }
        
        GameUser[] users = context.Database.GetUsersWithStatisticsNeedingUpdates().ToArray();

        foreach (GameUser user in users)
        {
            context.Database.RecalculateUserStatistics(user);
        }

        int updated = levels.Length + users.Length;
        if(updated > 0)
            context.Logger.LogInfo(RefreshContext.Worker, $"Recalculated statistics for {updated} objects");
    }
}