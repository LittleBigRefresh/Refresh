using System.Diagnostics.CodeAnalysis;
using Refresh.Core;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Repeating;

public class ObjectStatisticsJob : RepeatingJob
{
    protected override int Interval => 60_000;

    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
    public override void ExecuteJob(WorkContext context)
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

        GamePlaylist[] playlists = context.Database.GetPlaylistsWithStatisticsNeedingUpdates()
            .Take(500)
            .ToArray();

        foreach (GamePlaylist playlist in playlists)
        {
            context.Database.RecalculatePlaylistStatistics(playlist);
        }

        int updated = levels.Length + users.Length + playlists.Length;
        if(updated > 0)
            context.Logger.LogInfo(RefreshContext.Worker, $"Recalculated statistics for {updated} objects");
    }
}