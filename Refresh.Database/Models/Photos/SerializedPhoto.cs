using System.Xml.Serialization;
using Refresh.Database.Query;

namespace Refresh.Database.Models.Photos;

#nullable disable

[XmlRoot("photo")]
[XmlType("photo")]
public class SerializedPhoto : IPhotoUpload
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
    [XmlIgnore] public int LevelId => this.Level.LevelId;
    [XmlIgnore] public string LevelType => this.Level.Type;
    [XmlIgnore] public string LevelTitle => this.Level.Title;
    
    [XmlArray("subjects")] public List<SerializedPhotoSubject> PhotoSubjects { get; set; } = [];
}