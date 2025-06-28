using System.Diagnostics.CodeAnalysis;
using Refresh.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.Workers.Workers;

public class ObjectStatisticsWorker : IWorker
{
    public int WorkInterval => 60_000;

    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
    public void DoWork(DataContext context)
    {
        GameLevel[] levels = context.Database.GetLevelsWithStatisticsNeedingUpdates()
            .Take(500)
            .ToArray();

        foreach (GameLevel level in levels)
        {
            context.Database.RecalculateLevelStatistics(level);
        }
        
        GameUser[] users = context.Database.GetUsersWithStatisticsNeedingUpdates()
            .Take(500)
            .ToArray();

        foreach (GameUser user in users)
        {
            context.Database.RecalculateUserStatistics(user);
        }

        int updated = levels.Length + users.Length;
        if(updated > 0)
            context.Logger.LogInfo(RefreshContext.Worker, $"Recalculated statistics for {updated} objects");
    }
}