using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Pins;
using System.Collections.Frozen;

namespace Refresh.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdateUserPinProgress(Dictionary<long, int> pinProgressUpdates, GameUser user, TokenGame game)
    {
        DateTimeOffset now = this._time.Now;
        bool isBeta = game == TokenGame.BetaBuild;
        IEnumerable<PinProgressRelation> existingProgresses = this.GetPinProgressesByUser(user, isBeta);
        FrozenSet<long> specialTreatmentPins =
        [
            (long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores,
            (long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores,
        ];

        this.Write(() => 
        {
            foreach (KeyValuePair<long, int> pinProgressUpdate in pinProgressUpdates)
            {
                long pinId = pinProgressUpdate.Key;
                int newProgress = pinProgressUpdate.Value;
                PinProgressRelation? existingProgress = existingProgresses.FirstOrDefault(p => p.PinId == pinId);

                if (existingProgress == null)
                {
                    PinProgressRelation newRelation = new()
                    {
                        PinId = pinId,
                        Progress = newProgress,
                        Publisher = user,
                        FirstPublished = now,
                        LastUpdated = now,
                        IsBeta = isBeta,
                    };
                    this.PinProgressRelations.Add(newRelation);
                    continue;
                }

                bool isSpecialTreatmentPin = specialTreatmentPins.Contains(pinId);

                // Only update progress if it's better. For most pins it's better the greater it is, but for the pins in
                // specialTreatmentPins, it's better the smaller it is.
                if (isSpecialTreatmentPin && newProgress < existingProgress.Progress
                || !isSpecialTreatmentPin && newProgress > existingProgress.Progress)
                {
                    existingProgress.Progress = newProgress;
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

    public PinProgressRelation UpdateUserPinProgressToLowest(long pinId, int newProgressValue, GameUser user, bool isBeta)
    {
        // Get pin progress if it exists already
        PinProgressRelation? progressToUpdate = this.PinProgressRelations.FirstOrDefault(p => p.PinId == pinId && p.PublisherId == user.UserId && p.IsBeta == isBeta);
        DateTimeOffset now = this._time.Now;

        if (progressToUpdate == null)
        {
            this.Write(() =>
            {
                //PinProgressRelation newRelation = new()
                progressToUpdate = new()
                {
                    PinId = pinId,
                    Progress = newProgressValue,
                    Publisher = user,
                    PublisherId = user.UserId,
                    FirstPublished = now,
                    LastUpdated = now,
                    IsBeta = isBeta,
                };

                this.PinProgressRelations.Add(progressToUpdate);
            });
        }
        // Only update if the final progress value is actually lower to the one already set
        else if (newProgressValue < progressToUpdate.Progress)
        {
            this.Write(() =>
            {
                progressToUpdate.Progress = newProgressValue;
                progressToUpdate.LastUpdated = now;
            });
        }
        
        return progressToUpdate!;
    }

    public PinProgressRelation IncrementUserPinProgress(long pinId, int progressToAdd, GameUser user, bool isBeta)
    {
        // Get pin progress if it exists already
        PinProgressRelation? progressToUpdate = this.PinProgressRelations.FirstOrDefault(p => p.PinId == pinId && p.PublisherId == user.UserId && p.IsBeta == isBeta);
        DateTimeOffset now = this._time.Now;

        if (progressToUpdate == null)
        {
            PinProgressRelation newRelation = new()
            {
                PinId = pinId,
                Progress = progressToAdd,
                Publisher = user,
                PublisherId = user.UserId,
                FirstPublished = now,
                LastUpdated = now,
                IsBeta = isBeta,
            };

            this.Write(() =>
            {
                this.PinProgressRelations.Add(newRelation);
            });

            return newRelation;
        }
        else
        {
            this.Write(() =>
            {
                progressToUpdate.Progress =+ progressToAdd;
                progressToUpdate.LastUpdated = now;
            });
        }
        
        return progressToUpdate;
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta)
        => this.PinProgressRelations
            .Where(p => p.Publisher == user && p.IsBeta == isBeta)
            .OrderByDescending(p => p.LastUpdated);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetPinProgressesByUser(user, game == TokenGame.BetaBuild), skip, count);

    public PinProgressRelation? GetUserPinProgress(long pinId, GameUser user, bool isBeta)
        => this.PinProgressRelations.FirstOrDefault(p => p.PinId == pinId && p.PublisherId == user.UserId && p.IsBeta == isBeta);

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game)
        => this.ProfilePinRelations
            .Where(p => p.Publisher == user && p.Game == game)
            .OrderBy(p => p.Index);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game), skip, count);
}