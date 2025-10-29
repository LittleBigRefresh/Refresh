using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Pins;

namespace Refresh.Database;

public partial class GameDatabaseContext // Pins
{
    public void UpdateUserPinProgress(Dictionary<long, int> pinProgressUpdates, GameUser user, bool isBeta, TokenPlatform platform)
    {
        DateTimeOffset now = this._time.Now;
        IEnumerable<PinProgressRelation> existingProgresses = this.GetPinProgressesByUser(user, isBeta, platform);
        List<long> descendingProgressPins =
        [
            (long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores,
            (long)ServerPins.TopXOfAnyCommunityLevelWithOver50Scores,
        ];

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
                    Platform = platform,
                };
                this.PinProgressRelations.Add(newRelation);
                continue;
            }

            bool isSpecialTreatmentPin = descendingProgressPins.Contains(pinId);

            // Only update progress if it's better. For most pins it's better the greater it is, but for the pins in
            // specialTreatmentPins, it's better the smaller it is.
            if ((!isSpecialTreatmentPin && newProgress > existingProgress.Progress)
                || (isSpecialTreatmentPin && newProgress < existingProgress.Progress))
            {
                existingProgress.Progress = newProgress;
                existingProgress.LastUpdated = now;
            }
        }
        this.SaveChanges();
    }

    public void UpdateUserProfilePins(List<long> pinUpdates, GameUser user, TokenGame game, TokenPlatform platform)
    {
        IEnumerable<long> existingProgressIds = this
            .GetPinProgressesByUser(user, game == TokenGame.BetaBuild, platform)
            .Select(p => p.PinId);
        IEnumerable<ProfilePinRelation> existingProfilePins = this.GetProfilePinsByUser(user, game, platform);
        DateTimeOffset now = this._time.Now;

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
                    Platform = platform,
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
        this.SaveChanges();
    }

    public PinProgressRelation UpdateUserPinProgressToLowest(long pinId, int newProgressValue, GameUser user, bool isBeta, TokenPlatform platform)
    {
        // Get pin progress if it exists already
        PinProgressRelation? progressToUpdate = this.GetUserPinProgress(pinId, user, isBeta, platform);
        DateTimeOffset now = this._time.Now;

        if (progressToUpdate == null)
        {
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
            SaveChanges();
        }
        // Only update if the final progress value is actually lower to the one already set
        else if (newProgressValue < progressToUpdate.Progress)
        {
            progressToUpdate.Progress = newProgressValue;
            progressToUpdate.LastUpdated = now;
            SaveChanges();
        }
        
        return progressToUpdate!;
    }

    public PinProgressRelation IncrementUserPinProgress(long pinId, int progressToAdd, GameUser user, bool isBeta, TokenPlatform platform)
    {
        // Get pin progress if it exists already
        PinProgressRelation? progressToUpdate = this.GetUserPinProgress(pinId, user, isBeta, platform);
        DateTimeOffset now = this._time.Now;

        if (progressToUpdate == null)
        {
            progressToUpdate = new()
            {
                PinId = pinId,
                Progress = progressToAdd,
                Publisher = user,
                PublisherId = user.UserId,
                FirstPublished = now,
                LastUpdated = now,
                IsBeta = isBeta,
                Platform = platform,
            };
            this.PinProgressRelations.Add(progressToUpdate);
        }
        else
        {
            progressToUpdate.Progress += progressToAdd;
            progressToUpdate.LastUpdated = now;
        }
        
        this.SaveChanges();
        return progressToUpdate;
    }

    private IEnumerable<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta, TokenPlatform platform)
        => this.PinProgressRelations
            .Where(p => p.PublisherId == user.UserId && (p.IsBeta == isBeta && p.Platform == platform || p.Platform == TokenPlatform.Website))
            .OrderByDescending(p => p.LastUpdated);
    
    public DatabaseList<PinProgressRelation> GetPinProgressesByUser(GameUser user, bool isBeta, TokenPlatform platform, int skip, int count)
        => new(this.GetPinProgressesByUser(user, isBeta, platform), skip, count);

    public PinProgressRelation? GetUserPinProgress(long pinId, GameUser user, bool isBeta, TokenPlatform platform)
        => this.PinProgressRelations.FirstOrDefault(p => p.PinId == pinId && p.PublisherId == user.UserId 
            && (p.IsBeta == isBeta && p.Platform == platform || p.Platform == TokenPlatform.Website));

    private IEnumerable<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, TokenPlatform platform)
        => this.ProfilePinRelations
            .Where(p => p.PublisherId == user.UserId && p.Game == game && p.Platform == platform)
            .OrderBy(p => p.Index);

    public DatabaseList<ProfilePinRelation> GetProfilePinsByUser(GameUser user, TokenGame game, TokenPlatform platform, int skip, int count)
        => new(this.GetProfilePinsByUser(user, game, platform), skip, count);
    
    public void AddPinProgress(PinProgressRelation relation, bool save)
    {
        this.PinProgressRelations.Add(relation);
        if (save) this.SaveChanges();
    }

    public void RemovePinProgresses(IEnumerable<PinProgressRelation> relations, bool save)
    {
        this.PinProgressRelations.RemoveRange(relations);
        if (save) this.SaveChanges();
    }
}