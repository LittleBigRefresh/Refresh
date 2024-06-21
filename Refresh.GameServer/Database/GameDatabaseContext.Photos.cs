using JetBrains.Annotations;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.UserData;
using GamePhoto = Refresh.GameServer.Types.Photos.GamePhoto;
using GamePhotoSubject = Refresh.GameServer.Types.Photos.GamePhotoSubject;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Photos
{
    public void UploadPhoto(SerializedPhoto photo, GameUser publisher)
    {
        GamePhoto newPhoto = new()
        {
            SmallAsset = this.GetAssetFromHash(photo.SmallHash) ?? throw new Exception($"Small asset {photo.SmallHash} is missing!"),
            MediumAsset = this.GetAssetFromHash(photo.MediumHash) ?? throw new Exception($"Medium asset {photo.MediumHash} is missing!"),
            LargeAsset = this.GetAssetFromHash(photo.LargeHash) ?? throw new Exception($"Large asset {photo.LargeHash} is missing!"),
            PlanHash = photo.PlanHash,
            
            Publisher = publisher,
            LevelName = photo.Level?.Title ?? "",
            LevelType = photo.Level?.Type ?? "",
            //If level is null, default to level ID 0
            LevelId = photo.Level?.LevelId ?? 0,

            TakenAt = DateTimeOffset.FromUnixTimeSeconds(Math.Clamp(photo.Timestamp, this._time.EarliestDate, this._time.TimestampSeconds)),
            PublishedAt = this._time.Now,
        };

        if (photo.Level?.Type == "user") 
            newPhoto.Level = this.GetLevelById(photo.Level.LevelId);

        float[] bounds = new float[SerializedPhotoSubject.FloatCount];

        List<GamePhotoSubject> gameSubjects = new(photo.PhotoSubjects.Count);
        foreach (SerializedPhotoSubject subject in photo.PhotoSubjects)
        {
            GameUser? subjectUser = null;
            
            if (!string.IsNullOrEmpty(subject.Username)) 
                subjectUser = this.GetUserByUsername(subject.Username);
            
            SerializedPhotoSubject.ParseBoundsList(subject.BoundsList, bounds);

            gameSubjects.Add(new GamePhotoSubject(subjectUser, subject.DisplayName, bounds));
        }

        newPhoto.Subjects = gameSubjects;

        this.AddSequentialObject(newPhoto);
    }

    public void RemovePhoto(GamePhoto photo)
    {
        this.Write(() =>
        {
            this.GamePhotos.Remove(photo);
        });
    }

    public int GetTotalPhotoCount() => this.GamePhotos.Count();
    
    [Pure]
    public DatabaseList<GamePhoto> GetRecentPhotos(int count, int skip) =>
        new(this.GamePhotos
            .OrderByDescending(p => p.PublishedAt), skip, count);

    [Pure]
    public GamePhoto? GetPhotoById(int id) =>
        this.GamePhotos.FirstOrDefault(p => p.PhotoId == id);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosByUser(GameUser user, int count, int skip) =>
        new(this.GamePhotos.Where(p => p.Publisher == user)
            .OrderByDescending(p => p.TakenAt), skip, count);
    
    [Pure]
    public int GetTotalPhotosByUser(GameUser user)
        => this.GamePhotos
            .Count(p => p.Publisher == user);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosWithUser(GameUser user, int count, int skip) =>
        new(this.GamePhotos
            // FIXME: client-side enumeration
            .AsEnumerable()
            .Where(p => p.Subjects.FirstOrDefault(s => Equals(s.User, user)) != null)
            .OrderByDescending(p => p.TakenAt)
            .Skip(skip)
            .Take(count));
    
    [Pure]
    public int GetTotalPhotosWithUser(GameUser user)
        => this.GamePhotos
            // FIXME: client-side enumeration
            .AsEnumerable()
            .Count(p => p.Subjects.FirstOrDefault(s => Equals(s.User, user)) != null);

    [Pure]
    public DatabaseList<GamePhoto> GetPhotosInLevel(GameLevel level, int count, int skip) =>
        new(this.GamePhotos.Where(p => p.LevelId == level.LevelId), skip, count);
    
    [Pure]
    public int GetTotalPhotosInLevel(GameLevel level)
        => this.GamePhotos.Count(p => p.LevelId == level.LevelId);
}