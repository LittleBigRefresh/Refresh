using System.Xml.Serialization;
using Refresh.Database.Query;

namespace Refresh.Database.Models.Photos;

#nullable disable

[XmlRoot("subject")]
[XmlType("subject")]
public class SerializedPhotoSubject : IPhotoUploadSubject
{
    [XmlElement("npHandle")]
    public string Username { get; set; }
    
    [XmlElement("displayName")]
    public string DisplayName { get; set; }
    
    [XmlElement("bounds")]
    public string BoundsList { get; set; }
}