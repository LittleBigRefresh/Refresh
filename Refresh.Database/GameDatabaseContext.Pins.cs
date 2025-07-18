using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;

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

    public void UpdateUserProfilePins(List<long> pinUpdates, GameUser user, TokenGame game)
    {
        IEnumerable<long> existingProgressIds = this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild).Select(p => p.PinId);
        IEnumerable<ProfilePinRelation> existingProfilePins = this.GetProfilePinsByUser(user, game);
        DateTimeOffset now = this._time.Now;

        this.Write(() =>
        {
            for (int i = 0; i < pinUpdates.Count; i++)
            {
                long progressType = pinUpdates[i];

                // Does the user have any progress on the new pin?
                if (!existingProgressIds.Contains(progressType)) continue;

                ProfilePinRelation? existingPinAtIndex = existingProfilePins.FirstOrDefault(p => p.Index == i);

                // If the pin at this position hasn't changed, skip it
                if (existingPinAtIndex?.PinId == progressType) continue;

                if (existingPinAtIndex == null)
                {
                    this.ProfilePinRelations.Add(new()
                    {
                        PinId = progressType,
                        Publisher = user,
                        PublisherId = user.UserId,
                        Index = i,
                        Game = game,
                        Timestamp = now,
                    });
                }
                else
                {
                    this.ProfilePinRelations.Update(existingPinAtIndex);
                    existingPinAtIndex.PinId = progressType;
                    existingPinAtIndex.Timestamp = now; // New pin at this position: reset timestamp
                }
            }
        });
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations
            .Where(p => p.Publisher == user && p.IsBeta == isBeta)
            .OrderByDescending(p => p.LastUpdated);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild), skip, count);

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game)
        => this.ProfilePinRelations
            .Where(p => p.Publisher == user && p.Game == game)
            .OrderBy(p => p.Index);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game), skip, count);
}