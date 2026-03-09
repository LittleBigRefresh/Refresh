using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Photos;

namespace Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

public class SerializedPhotoUploadEvent : SerializedLevelEvent
{
    [XmlElement("photo_id")] public int PhotoId { get; set; }
    [XmlElement("user_in_photo")] public List<string> UsersInPhoto = [];
    
    public static SerializedPhotoUploadEvent? FromSerializedLevelEvent(SerializedLevelEvent? e, GamePhoto? photo, DataContext dataContext)
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
            UsersInPhoto = dataContext.Database.GetSubjectsInPhoto(photo).Select(s => s.DisplayName).ToList(),
        };
    }
}