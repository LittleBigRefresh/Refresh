using System.Globalization;
using System.Xml.Serialization;

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
            SmallHash = photo.SmallHash,
            MediumHash = photo.MediumHash,
            LargeHash = photo.LargeHash,
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