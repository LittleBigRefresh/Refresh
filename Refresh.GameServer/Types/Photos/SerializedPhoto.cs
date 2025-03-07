using System.Xml.Serialization;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Photos;

#nullable disable

[XmlRoot("photo")]
[XmlType("photo")]
public class SerializedPhoto
{
    [XmlAttribute("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("id")]
    public int PhotoId { get; set; } = 0;

    [XmlElement("author")]
    public string AuthorName { get; set; } = "";
    
    [XmlElement("small")] public string SmallHash { get; set; }
    [XmlElement("medium")] public string MediumHash { get; set; }
    [XmlElement("large")] public string LargeHash { get; set; }
    [XmlElement("plan")] public string PlanHash { get; set; }
    
    [XmlElement("slot")] public SerializedLevelIdTypeName Level { get; set; }
    
    [XmlArray("subjects")] public List<SerializedPhotoSubject> PhotoSubjects { get; set; }

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