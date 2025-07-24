using Refresh.Core;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class EnsureDeletedUsersDeletedMigration : MigrationJob<GameUser>
{
    protected override int BatchCount => 5; // lower batch size as deletion takes a while

    protected override IQueryable<GameUser> SortAndFilter(IQueryable<GameUser> query)
    {
        return query
            .Where(u => u.BanReason == "This user's account has been deleted." || u.PasswordBcrypt == "deleted")
            .OrderBy(u => u.UserId); // can't use join date here as we're changing the join date when we delete data
    }

    protected override void Migrate(WorkContext context, GameUser[] batch)
    {
        foreach (GameUser user in batch)
        {
            context.Logger.LogWarning(RefreshContext.Worker, $"Deleting {user.Username}'s account again to ensure data has been wiped...");
            context.Database.DeleteUser(user);
        }
    }
}