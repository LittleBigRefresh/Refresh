using Refresh.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels;

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

        int updated = levels.Length;
        if(updated > 0)
            context.Logger.LogInfo(RefreshContext.Worker, $"Recalculated statistics for {updated} objects");
    }
}