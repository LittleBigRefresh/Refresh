using JetBrains.Annotations;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Query;
using Refresh.Database.Helpers;
using Bunkum.Core;

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
        .Include(p => p.Level!.Publisher!.Statistics);

    private IQueryable<GamePhotoSubject> GamePhotoSubjectsIncluded => this.GamePhotoSubjects
        .Include(p => p.User)
        .Include(p => p.User!.Statistics)
        .Include(p => p.Photo)
        .Include(p => p.Photo!.Publisher)
        .Include(p => p.Photo!.Publisher.Statistics)
        .Include(p => p.Photo!.Level)
        .Include(p => p.Photo!.Level!.Publisher)
        .Include(p => p.Photo!.Level!.Publisher!.Statistics)
        .Include(p => p.Photo!.LargeAsset)
        .Include(p => p.Photo!.MediumAsset)
        .Include(p => p.Photo!.SmallAsset);
    
    public GamePhoto UploadPhoto(IPhotoUpload photo, IEnumerable<IPhotoUploadSubject> subjects, GameUser publisher, GameLevel? level)
    {
        GamePhoto newPhoto = new()
        {
            SmallAsset = this.GetAssetFromHash(photo.SmallHash) ?? throw new Exception($"Small asset {photo.SmallHash} is missing!"),
            MediumAsset = this.GetAssetFromHash(photo.MediumHash) ?? throw new Exception($"Medium asset {photo.MediumHash} is missing!"),
            LargeAsset = this.GetAssetFromHash(photo.LargeHash) ?? throw new Exception($"Large asset {photo.LargeHash} is missing!"),
            PlanHash = photo.PlanHash,
            
            Publisher = publisher,
            Level = level,
            LevelType = photo.LevelType ?? "",
            OriginalLevelId = photo.LevelId ?? 0,
            OriginalLevelName = photo.LevelTitle ?? "",

            TakenAt = DateTimeOffset.FromUnixTimeSeconds(Math.Clamp(photo.Timestamp, this._time.EarliestDate, this._time.TimestampSeconds)),
            PublishedAt = this._time.Now,
        };

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

        List<IPhotoUploadSubject> subjectsList = subjects.ToList();
        List<GamePhotoSubject> finalSubjectsList = [];

        // Take care of subjects after saving the photo itself to keep the photo, even if its subjects are malformed
        for (int i = 0; i < subjectsList.Count; i++)
        {
            IPhotoUploadSubject subject = subjectsList[i];

            float[] bounds = new float[PhotoHelper.SubjectBoundaryCount];
            try
            {
                bounds = PhotoHelper.ParseBoundsList(subject.BoundsList);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(BunkumCategory.UserPhotos, $"Could not parse {subject.DisplayName}'s photo bounds: {ex.GetType()} - {ex.Message}");
            }

            GameUser? subjectUser = string.IsNullOrWhiteSpace(subject.Username) ? null : this.GetUserByUsername(subject.Username);
            finalSubjectsList.Add(new()
            {
                Photo = newPhoto,
                User = subjectUser,
                DisplayName = subject.DisplayName,
                PlayerId = i + 1, // Player number 1 - 4
                Bounds = bounds
            });

            if (subjectUser != null)
            {
                this.WriteEnsuringStatistics(subjectUser, () =>
                {
                    subjectUser.Statistics!.PhotosWithUserCount++;
                });
            }
        }

        this.GamePhotoSubjects.AddRange(finalSubjectsList);
        this.SaveChanges();
        
        this.CreatePhotoUploadEvent(publisher, newPhoto);
        return newPhoto;
    }

    /// <remarks>
    /// Migration only!!
    /// </remarks>
    public void MigratePhotoSubjects(GamePhoto photo, bool saveChanges)
    {
        List<GamePhotoSubject> subjects = [];

#pragma warning disable CS0618 // obsoletion
        
        // If DisplayName is not null, there is a subject in that spot
        if (photo.Subject1DisplayName != null)
        {
            subjects.Add(new()
            {
                Photo = photo,
                PlayerId = 1,
                DisplayName = photo.Subject1DisplayName,
                User = photo.Subject1User,
                Bounds = photo.Subject1Bounds,
            });
        }

        if (photo.Subject2DisplayName != null)
        {
            subjects.Add(new()
            {
                Photo = photo,
                PlayerId = 2,
                DisplayName = photo.Subject2DisplayName,
                User = photo.Subject2User,
                Bounds = photo.Subject2Bounds,
            });
        }

        if (photo.Subject3DisplayName != null)
        {
            subjects.Add(new()
            {
                Photo = photo,
                PlayerId = 3,
                DisplayName = photo.Subject3DisplayName,
                User = photo.Subject3User,
                Bounds = photo.Subject3Bounds,
            });
        }

        if (photo.Subject4DisplayName != null)
        {
            subjects.Add(new()
            {
                Photo = photo,
                PlayerId = 4,
                DisplayName = photo.Subject4DisplayName,
                User = photo.Subject4User,
                Bounds = photo.Subject4Bounds,
            });
        }
#pragma warning restore CS0618

        this.GamePhotoSubjects.AddRange(subjects);
        if (saveChanges) this.SaveChanges();
    }

    public void RemovePhoto(GamePhoto photo)
    {
        foreach (GameUser subjectUser in this.GetUsersInPhoto(photo).ToArray())
        {
            this.WriteEnsuringStatistics(subjectUser, () =>
            {
                subjectUser.Statistics!.PhotosWithUserCount--;
            });
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

            // Remove all subjects
            this.GamePhotoSubjects.RemoveRange(s => s.PhotoId == photo.PhotoId);
            
            // Remove the photo
            this.GamePhotos.Remove(photo);

            photo.Publisher.Statistics!.PhotosByUserCount--;
        });
    }

    public IQueryable<GamePhotoSubject> GetSubjectsInPhoto(GamePhoto photo)
        => this.GamePhotoSubjectsIncluded.Where(s => s.PhotoId == photo.PhotoId);
    
    public IQueryable<GameUser> GetUsersInPhoto(GamePhoto photo)
        => this.GetSubjectsInPhoto(photo)
            .Where(s => s.User != null)
            .Select(s => s.User!);

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
        new(this.GamePhotoSubjectsIncluded
            .Where(s => s.UserId == user.UserId)
            .Select(s => s.Photo), skip, count);
    
    [Pure]
    public int GetTotalPhotosWithUser(GameUser user)
        => this.GamePhotoSubjects.Count(s => s.UserId == user.UserId);

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