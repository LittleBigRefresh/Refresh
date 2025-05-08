using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Photos;

namespace Refresh.GameServer.Extensions;

public static class PhotoExtensions
{
    public static SerializedPhoto FromGamePhoto(GamePhoto photo, DataContext dataContext)
    {
        SerializedPhoto newPhoto = new()
        {
            PhotoId = photo.PhotoId,
            AuthorName = photo.Publisher.Username,
            Timestamp = photo.TakenAt.ToUnixTimeMilliseconds(),
            // NOTE: we usually would do `if psp, prepend psp/ to the hashes`,
            // but since we are converting the psp TGA assets to PNG in FillInExtraData, we don't need to!
            // also, I think the game would get mad if we did that
            LargeHash = dataContext.Database.GetAssetFromHash(photo.LargeAsset.AssetHash)?.GetAsPhoto(dataContext.Game, dataContext) ?? photo.LargeAsset.AssetHash,
            MediumHash = dataContext.Database.GetAssetFromHash(photo.MediumAsset.AssetHash)?.GetAsPhoto(dataContext.Game, dataContext) ?? photo.MediumAsset.AssetHash,
            SmallHash = dataContext.Database.GetAssetFromHash(photo.SmallAsset.AssetHash)?.GetAsPhoto(dataContext.Game, dataContext) ?? photo.SmallAsset.AssetHash,
            PlanHash = photo.PlanHash,
            PhotoSubjects = new List<SerializedPhotoSubject>(photo.Subjects.Count),
        };
        
        foreach (GamePhotoSubject subject in photo.Subjects)
        {
            SerializedPhotoSubject newSubject = new()
            {
                Username = subject.User?.Username ?? subject.DisplayName,
                DisplayName = subject.DisplayName,
                BoundsList = string.Join(',', subject.Bounds),
            };

            newPhoto.PhotoSubjects.Add(newSubject);
        }

        return newPhoto;
    }
}