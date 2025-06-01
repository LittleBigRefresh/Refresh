using System.Xml.Serialization;
using Refresh.Database.Models.Photos;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedPhotoUploadEvent : SerializedLevelEvent
{
    [XmlElement("photo_id")] public int PhotoId { get; set; }
    [XmlElement("user_in_photo")] public List<string> UsersInPhoto = [];
    
    public static SerializedPhotoUploadEvent? FromSerializedLevelEvent(SerializedLevelEvent? e, GamePhoto? photo)
    {
        if (e == null || photo == null)
            return null;
        
        return new SerializedPhotoUploadEvent
        {
            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,

            PhotoId = photo.PhotoId,
            UsersInPhoto = photo.Subjects.Select(s => s.DisplayName).ToList(),
        };
    }
}