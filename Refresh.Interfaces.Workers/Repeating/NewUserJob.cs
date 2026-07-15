using Refresh.Common;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Repeating;

/// <summary>
/// A worker that handles setting new users as regular users, depending on their account age and the server config.
/// </summary>
// TODO also set users back as "new" if duration in config is updated to result in user being "new" again
public class NewUserJob : RepeatingJob
{
    private int _requiredAccountAge;
    protected override int Interval => 60_000 * 5; // 5 minutes, no need to execute too often
    
    public NewUserJob(int requiredAccountAge)
    {
        this._requiredAccountAge = requiredAccountAge;
    }

    public override void ExecuteJob(WorkContext context)
    {
        DateTimeOffset now = DateTimeOffset.Now; // TODO use IDateTimeProvider for getting time in jobs
        DatabaseList<GameUser> newUsers = context.Database.GetAllUsersWithRole(GameUserRole.NewUser);

        foreach (GameUser user in newUsers.Items)
        {
            // If an account is, e.g., 2 hours and 40 minutes old, and max age for new users is 3 hours, we wouldn't
            // consider max to be reached yet, so floor the difference.
            long accountAge = (long)Math.Floor(now.Subtract(user.JoinDate).TotalHours);
            if (accountAge < this._requiredAccountAge) continue; // Don't promote user if they haven't reached max age yet
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Promoting {user} to regular user since their account is {accountAge}/{this._requiredAccountAge} hours old now.");
            context.Database.SetUserRole(user, GameUserRole.User);
        }
    }
}