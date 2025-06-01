using System.Xml.Serialization;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

[XmlType("event")]
[XmlRoot("event")]
[XmlInclude(typeof(SerializedUserEvent))]
[XmlInclude(typeof(SerializedLevelEvent))]
[XmlInclude(typeof(SerializedLevelUploadEvent))]
[XmlInclude(typeof(SerializedLevelPlayEvent))]
[XmlInclude(typeof(SerializedScoreSubmitEvent))]
[XmlInclude(typeof(SerializedPhotoUploadEvent))]
public abstract class SerializedEvent : IDataConvertableFrom<SerializedEvent, Event>
{
    [XmlAttribute("type")]
    public EventType Type { get; set; }
    [XmlElement("timestamp")]
    public long Timestamp { get; set; }
    [XmlElement("actor")]
    public string Actor { get; set; } = string.Empty;

    public static SerializedEvent? FromOld(Event? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        GameUser? user = null;
        GameLevel? level = null;
        GameSubmittedScore? score = null;
        GamePhoto? photo = null;

        if (old.StoredDataType == EventDataType.User)
            user = dataContext.Database.GetUserFromEvent(old);
        else if (old.StoredDataType == EventDataType.Level)
            level = dataContext.Database.GetLevelFromEvent(old);
        else if (old.StoredDataType == EventDataType.Score)
        {
            score = dataContext.Database.GetScoreFromEvent(old);
            level = score?.Level;
        }
        else if (old.StoredDataType == EventDataType.Photo)
        {
            photo = dataContext.Database.GetPhotoFromEvent(old);
            level = photo?.Level;
        }

        switch (old.EventType)
        {
            case EventType.LevelFavourite:
            case EventType.LevelUnfavourite:
            case EventType.LevelStarRate:
            case EventType.LevelTag:
            case EventType.PostLevelComment:
            case EventType.DeleteLevelComment:
            case EventType.LevelUnpublish:
            case EventType.LevelTeamPick:
            case EventType.LevelRate:
            case EventType.LevelReview:
            case EventType.PlaylistCreate:
            case EventType.PlaylistFavourite:
            case EventType.PlaylistAddLevel:
                return FromOldLevelEvent(old, level!);
            case EventType.UserFavourite:
            case EventType.UserUnfavourite:
            case EventType.PostUserComment:
            case EventType.UserFirstLogin:
                return FromOldUserEvent(old, user!);
            case EventType.LevelUpload:
                return SerializedLevelUploadEvent.FromSerializedLevelEvent(FromOldLevelEvent(old, level!));
            case EventType.LevelPlay:
                return SerializedLevelPlayEvent.FromSerializedLevelEvent(FromOldLevelEvent(old, level!));
            case EventType.PhotoUpload:
                return SerializedPhotoUploadEvent.FromSerializedLevelEvent(FromOldLevelEvent(old, level!), photo!);
            case EventType.LevelScore:
                return SerializedScoreSubmitEvent.FromSerializedLevelEvent(FromOldLevelEvent(old, level!), score!);
            case EventType.NewsPost:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static SerializedLevelEvent FromOldLevelEvent(Event old, GameLevel level)
    {
        return new SerializedLevelEvent
        {
            Timestamp = old.Timestamp.ToUnixTimeMilliseconds(),
            Type = old.EventType,
            Actor = old.User.Username,
            LevelId = new SerializedLevelId()
            {
                LevelId = level.LevelId,
                Type = level.SlotType.ToGameType(),
            },
        };
    }
    
    private static SerializedUserEvent FromOldUserEvent(Event old, GameUser user)
    {
        return new SerializedUserEvent
        {
            Timestamp = old.Timestamp.ToUnixTimeMilliseconds(),
            Type = old.EventType,
            Actor = old.User.Username,
            Username = user.Username,
        };
    }

    public static IEnumerable<SerializedEvent> FromOldList(IEnumerable<Event> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}