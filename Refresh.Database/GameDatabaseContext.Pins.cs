using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Pins;
using Refresh.GameServer.Types.UserData;

namespace Refresh.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdateUserPinProgress(Dictionary<long, int> pinProgressUpdates, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        bool isBeta = game == TokenGame.BetaBuild;
        IEnumerable<PinProgressRelation> existingProgresses = this.GetPinProgressesByUser(user, isBeta);
       
        this.Write(() => 
        {
            foreach (KeyValuePair<long, int> pinProgressUpdate in pinProgressUpdates)
            {
                PinProgressRelation? existingProgress = existingProgresses.FirstOrDefault(p => p.PinId == pinProgressUpdate.Key);

                if (existingProgress == null)
                {
                    PinProgressRelation newRelation = new()
                    {
                        PinId = pinProgressUpdate.Key,
                        Progress = pinProgressUpdate.Value,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    };
                    this.PinProgressRelations.Add(newRelation);
                }
                // Only update if the new progress is actually better
                else if (pinProgressUpdate.Value > existingProgress.Progress)
                {
                    existingProgress.Progress = pinProgressUpdate.Value;
                    existingProgress.LastUpdated = now;
                }
            }
        });
    }

    public void UpdateUserProfilePins(List<long> profilePinUpdates, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        IEnumerable<PinProgressRelation> existingProgresses = this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild);
        IEnumerable<ProfilePinRelation> existingProfilePins = this.GetProfilePinsByUser(user, game);
        int failedProfilePinUpdates = 0;

        this.Write(() => 
        {
            for (int i = 0; i < profilePinUpdates.Count; i++)
            {
                ProfilePinRelation? existingProfilePin = existingProfilePins.FirstOrDefault(p => p.Index == i);
                long progressType = profilePinUpdates[i];

                // If there is no profile pin at index i, or the existing profile pin at that index is
                // referencing a different pin, overwrite it
                if (existingProfilePin == null || existingProfilePin.PinId != progressType)
                {
                    // Does the user even have any progress on this pin?
                    if (existingProgresses.Any(p => p.PinId == progressType))
                    {
                        if (existingProfilePin != null) 
                        {
                            this.ProfilePinRelations.Remove(existingProfilePin);
                        }

                        ProfilePinRelation newProfilePin = new()
                        {
                            PinId = progressType,
                            Publisher = user,
                            Index = i,
                            Game = game,
                            Timestamp = now,
                        };
                        this.ProfilePinRelations.Add(newProfilePin);
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
                $"Failed to update {failedProfilePinUpdates} out of {profilePinUpdates.Count} profile pins "+
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