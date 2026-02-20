using JetBrains.Annotations;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;

namespace Refresh.Database;

public partial class GameDatabaseContext // Photos
{
    private IQueryable<GamePhoto> GamePhotosIncluded => this.GamePhotos
        .Include(p => p.LargeAsset)
        .Include(p => p.MediumAsset)
        .Include(p => p.SmallAsset)
        .Include(p => p.Publisher)
        .Include(p => p.Publisher.Statistics)
        .Include(p => p.Level)
        .Include(p => p.Level!.Publisher)
        .Include(p => p.Level!.Publisher!.Statistics)
        .Include(p => p.Subject1User)
        .Include(p => p.Subject1User!.Statistics)
        .Include(p => p.Subject2User)
        .Include(p => p.Subject2User!.Statistics)
        .Include(p => p.Subject3User)
        .Include(p => p.Subject3User!.Statistics)
        .Include(p => p.Subject4User)
        .Include(p => p.Subject4User!.Statistics);
    
    public void UploadPhoto(SerializedPhoto photo, GameUser publisher)
    {
        GamePhoto newPhoto = new()
        {
            SmallAsset = this.GetAssetFromHash(photo.SmallHash) ?? throw new Exception($"Small asset {photo.SmallHash} is missing!"),
            MediumAsset = this.GetAssetFromHash(photo.MediumHash) ?? throw new Exception($"Medium asset {photo.MediumHash} is missing!"),
            LargeAsset = this.GetAssetFromHash(photo.LargeHash) ?? throw new Exception($"Large asset {photo.LargeHash} is missing!"),
            PlanHash = photo.PlanHash,
            
            Publisher = publisher,
            LevelType = photo.Level?.Type ?? "",
            OriginalLevelId = photo.Level?.LevelId ?? 0,
            OriginalLevelName = photo.Level?.Title ?? "",

            TakenAt = DateTimeOffset.FromUnixTimeSeconds(Math.Clamp(photo.Timestamp, this._time.EarliestDate, this._time.TimestampSeconds)),
            PublishedAt = this._time.Now,
        };

        GameLevel? level = null;

        if (photo.Level?.Type is "user" or "developer") 
        {
            level = this.GetLevelByIdAndType(photo.Level.Type, photo.Level.LevelId);
            newPhoto.Level = level;
        }

        float[] bounds = new float[SerializedPhotoSubject.FloatCount];

        List<GamePhotoSubject> gameSubjects = new(photo.PhotoSubjects.Count);
        foreach (SerializedPhotoSubject subject in photo.PhotoSubjects)
        {
            GameUser? subjectUser = null;
            
            if (!string.IsNullOrEmpty(subject.Username)) 
                subjectUser = this.GetUserByUsername(subject.Username);
            
            SerializedPhotoSubject.ParseBoundsList(subject.BoundsList, bounds);

            gameSubjects.Add(new GamePhotoSubject(subjectUser, subject.DisplayName, bounds));

            if (subjectUser != null)
            {
                this.WriteEnsuringStatistics(subjectUser, () =>
                {
                    subjectUser.Statistics!.PhotosWithUserCount++;
                });
            }
        }

        newPhoto.Subjects = gameSubjects;

        this.WriteEnsuringStatistics(publisher, () =>
        {
            this.GamePhotos.Add(newPhoto);
            publisher.Statistics!.PhotosByUserCount++;
        });

        if (level != null)
        {
            this.WriteEnsuringStatistics(publisher, level, () =>
            {
                level.Statistics!.PhotoInLevelCount++;
                if (level.Publisher?.UserId == publisher.UserId)
                {
                    level.Statistics!.PhotoByPublisherCount++;
                }
            });
        }
        
        this.CreatePhotoUploadEvent(publisher, newPhoto);
    }

    public void RemovePhoto(GamePhoto photo)
    {
        foreach (GamePhotoSubject subject in photo.Subjects)
        {
            if (subject.User != null)
            {
                this.WriteEnsuringStatistics(subject.User, () =>
                {
                    subject.User.Statistics!.PhotosWithUserCount--;
                });
            }
        }

        if (photo.Level != null)
        {
            this.WriteEnsuringStatistics(photo.Publisher, photo.Level, () =>
            {
                photo.Level.Statistics!.PhotoInLevelCount--;
                if (photo.Level.Publisher?.UserId == photo.PublisherId)
                {
                    photo.Level.Statistics!.PhotoByPublisherCount--;
                }
            });
        }

        this.WriteEnsuringStatistics(photo.Publisher, () =>
        {
            IQueryable<Event> photoEvents = this.Events
                .Where(e => e.StoredDataType == EventDataType.Photo && e.StoredSequentialId == photo.PhotoId);
                
            // Remove all events referencing the photo
            this.Events.RemoveRange(photoEvents);
            
            // Remove the photo
            this.GamePhotos.Remove(photo);

            photo.Publisher.Statistics!.PhotosByUserCount--;
        });
    }

    public int GetTotalPhotoCount() => this.GamePhotos.Count();
    
    [Pure]
    public DatabaseList<GamePhoto> GetRecentPhotos(int count, int skip) =>
        new(this.GamePhotosIncluded
            .OrderByDescending(p => p.PublishedAt), skip, count);

    [Pure]
    public GamePhoto? GetPhotoById(int id) =>
        this.GamePhotosIncluded.FirstOrDefault(p => p.PhotoId == id);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosByUser(GameUser user, int count, int skip) =>
        new(this.GamePhotosIncluded.Where(p => p.PublisherId == user.UserId)
            .OrderByDescending(p => p.TakenAt), skip, count);
    
    [Pure]
    public int GetTotalPhotosByUser(GameUser user)
        => this.GamePhotos
            .Count(p => p.PublisherId == user.UserId);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosWithUser(GameUser user, int count, int skip) =>
        new(this.GamePhotosIncluded
            .Where(p => p.Subject1UserId == user.UserId || p.Subject2UserId == user.UserId || p.Subject3UserId == user.UserId || p.Subject4UserId == user.UserId)
            .OrderByDescending(p => p.TakenAt), skip, count);
    
    [Pure]
    public int GetTotalPhotosWithUser(GameUser user)
        => this.GamePhotos
            .Count(p => p.Subject1UserId == user.UserId || p.Subject2UserId == user.UserId || p.Subject3UserId == user.UserId || p.Subject4UserId == user.UserId);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosInLevel(GameLevel level, int count, int skip)
        => new(this.GamePhotosIncluded
            .Where(p => p.LevelId == level.LevelId)
            .OrderByDescending(p => p.TakenAt), skip, count);
    
    [Pure]
    public int GetTotalPhotosInLevel(GameLevel level)
        => this.GamePhotos.Count(p => p.LevelId == level.LevelId);

    public DatabaseList<GamePhoto> GetPhotosInLevelByUser(GameLevel level, GameUser user, int count, int skip) 
        => new(this.GamePhotosIncluded
            .Where(p => p.LevelId == level.LevelId && p.PublisherId == user.UserId)
            .OrderByDescending(p => p.TakenAt), skip, count);

    public int GetTotalPhotosInLevelByUser(GameLevel level, GameUser? user)
    {
        if (user == null) return 0;
        return this.GamePhotos.Count(p => p.LevelId == level.LevelId && p.PublisherId == user.UserId);
    }

    public void DeletePhotosPostedByUser(GameUser user)
    {
        IEnumerable<GamePhoto> photos = this.GamePhotos.Where(s => s.PublisherId == user.UserId).ToArray();

        foreach (GamePhoto photo in photos)
        {
            // RemovePhoto already takes care of decrementing (effectively resetting) the publisher's photo count
            this.RemovePhoto(photo);
        }
    }
}