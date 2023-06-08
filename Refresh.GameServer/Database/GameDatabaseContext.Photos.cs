using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Photos;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Photos
{
    public void UploadPhoto(SerializedPhoto photo, GameUser publisher)
    {
        GamePhoto newPhoto = new()
        {
            SmallHash = photo.SmallHash,
            MediumHash = photo.MediumHash,
            LargeHash = photo.LargeHash,
            PlanHash = photo.PlanHash,
            
            Publisher = publisher,

            TakenAt = DateTimeOffset.FromUnixTimeSeconds(photo.Timestamp),
            PublishedAt = DateTimeOffset.Now,
        };
        
        float[] bounds = new float[SerializedPhotoSubject.FloatCount];
        foreach (SerializedPhotoSubject subject in photo.PhotoSubjects)
        {
            GamePhotoSubject newSubject = new();
            GameUser? subjectUser = null;
            
            if (!string.IsNullOrEmpty(subject.Username)) 
                subjectUser = this.GetUserByUsername(subject.Username);
            
            SerializedPhotoSubject.ParseBoundsList(subject.BoundsList, bounds);

            newSubject.User = subjectUser;
            newSubject.DisplayName = subject.DisplayName;
            foreach (float coord in bounds) newSubject.Bounds.Add(coord);
            
            newPhoto.Subjects.Add(newSubject);
        }

        this.AddSequentialObject(newPhoto);
    }
}