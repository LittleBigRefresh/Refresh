using Refresh.Core.Types.Data;
using Refresh.Database.Models.Photos;

namespace Refresh.Core.Extensions;

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
            LargeHash = photo.LargeAsset.GetAsPhoto(dataContext.Game, dataContext) ?? photo.LargeAsset.AssetHash,
            MediumHash = photo.MediumAsset.GetAsPhoto(dataContext.Game, dataContext) ?? photo.MediumAsset.AssetHash,
            SmallHash = photo.SmallAsset.GetAsPhoto(dataContext.Game, dataContext) ?? photo.SmallAsset.AssetHash,
            PlanHash = photo.PlanHash,
            PhotoSubjects = [],
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