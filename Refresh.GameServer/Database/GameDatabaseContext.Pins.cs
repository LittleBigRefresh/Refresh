using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Pins;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdateUserPinProgress(Dictionary<long, int> pinProgresses, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        bool isBeta = game == TokenGame.BetaBuild;
        IEnumerable<PinProgressRelation> exProgresses = this.GetPinProgressesByUser(user, isBeta);
       
        this.Write(() => 
        {
            foreach (KeyValuePair<long, int> pinProgress in pinProgresses)
            {
                PinProgressRelation? exRelation = exProgresses.FirstOrDefault(p => p.PinId == pinProgress.Key);

                if (exRelation == null)
                {
                    PinProgressRelation newRelation = new()
                    {
                        PinId = pinProgress.Key,
                        Progress = pinProgress.Value,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    };
                    this.PinProgressRelations.Add(newRelation);
                }
                // Only update if the new progress is actually better
                else if (pinProgress.Value > exRelation.Progress)
                {
                    exRelation.Progress = pinProgress.Value;
                    exRelation.LastUpdated = now;
                }
            }
        });
    }

    public void UpdateUserProfilePins(List<long> profilePins, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        IEnumerable<PinProgressRelation> exProgresses = this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild);
        IEnumerable<ProfilePinRelation> exProfilePins = this.GetProfilePinsByUser(user, game);
        int failedProfilePinUpdates = 0;

        this.Write(() => 
        {
            for (int i = 0; i < profilePins.Count; i++)
            {
                ProfilePinRelation? exRelation = exProfilePins.FirstOrDefault(p => p.Index == i);
                long progressType = profilePins[i];

                // If there is no profile pin at index i, or the existing profile pin at that index is
                // referencing a different pin, overwrite it
                if (exRelation == null || exRelation.PinId != progressType)
                {
                    // Does the user even have any progress on this pin?
                    if (exProgresses.Any(p => p.PinId == progressType))
                    {
                        if (exRelation != null) 
                        {
                            this.ProfilePinRelations.Remove(exRelation);
                        }

                        ProfilePinRelation newRelation = new()
                        {
                            PinId = progressType,
                            Publisher = user,
                            Index = i,
                            Game = game,
                            Timestamp = now,
                        };
                        this.ProfilePinRelations.Add(newRelation);
                    }
                    else
                    {
                        failedProfilePinUpdates++;
                    }
                }
            }
        });

        if (failedProfilePinUpdates > 0)
        {
            this.AddErrorNotification
            (
                "Profile pin update failed", 
                $"Failed to update {failedProfilePinUpdates} out of {profilePins.Count} profile pins "+
                $"for game {game} because we couldn't find your progress for these pins on the server.",
                user
            );
        }
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations
            .Where(p => p.Publisher == user && p.IsBeta == isBeta)
            .OrderByDescending(p => p.LastUpdated);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild), skip, count);

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game)
        => this.ProfilePinRelations
            .Where(p => p.Publisher == user && p._Game == (int)game)
            .OrderBy(p => p.Index);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game), skip, count);
}