using System.Xml.Serialization;
using Bunkum.Core.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;

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
    
    [XmlElement("slot")] public SerializedPhotoLevel Level { get; set; }
    
    [XmlArray("subjects")] public List<SerializedPhotoSubject> PhotoSubjects { get; set; }

    public static SerializedPhoto FromGamePhoto(GamePhoto photo)
    {
        SerializedPhoto newPhoto = new()
        {
            PhotoId = photo.PhotoId,
            AuthorName = photo.Publisher.Username,
            Timestamp = photo.TakenAt.ToUnixTimeMilliseconds(),
            //NOTE: we usually would do `if psp, prepend psp/ to the hashes`, but since we are converting the psp TGA assets to PNG in FillInExtraData, we dont need to! (also i think the game would get mad if we did that)
            SmallHash = photo.SmallAsset.AssetHash,
            MediumHash = photo.MediumAsset.AssetHash,
            LargeHash = photo.LargeAsset.AssetHash,
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

    public static SerializedPhoto FromGamePhotoWithExtraData(GamePhoto photo, GameDatabaseContext database, IDataStore dataStore, TokenGame game)
    {
        SerializedPhoto serializedPhoto = FromGamePhoto(photo);
        serializedPhoto.FillInExtraData(database, dataStore, game);
        return serializedPhoto;
    }

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore, TokenGame game)
    {
        this.LargeHash = database.GetAssetFromHash(this.LargeHash)?.GetAsPhoto(game, database, dataStore) ?? this.LargeHash;
        this.MediumHash = database.GetAssetFromHash(this.MediumHash)?.GetAsPhoto(game, database, dataStore) ?? this.MediumHash;
        this.SmallHash = database.GetAssetFromHash(this.SmallHash)?.GetAsPhoto(game, database, dataStore) ?? this.SmallHash;
    }
}