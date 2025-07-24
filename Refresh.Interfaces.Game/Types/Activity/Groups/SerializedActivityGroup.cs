using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.Game.Types.Activity.SerializedEvents;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Activity.Groups;

[XmlRoot("group")]
[XmlType("group")]
[XmlInclude(typeof(LevelSerializedActivityGroup))]
[XmlInclude(typeof(UserSerializedActivityGroup))]
public abstract class SerializedActivityGroup : IDataConvertableFrom<SerializedActivityGroup, DatabaseActivityGroup>
{
    [XmlAttribute("type")]
    public abstract string Type { get; set; }
    
    [XmlElement("timestamp")]
    public required long Timestamp { get; set; }
    
    [XmlElement("events")]
    public required Events? Events { get; set; }

    [XmlElement("subgroups")]
    public required Subgroups? Subgroups { get; set; }

    public static SerializedActivityGroup? FromOld(DatabaseActivityGroup? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        long timestamp = old.Timestamp.ToUnixTimeMilliseconds();

        Events events = new()
        {
            Items = SerializedEvent.FromOldList(old.Events, dataContext).ToList(),
        };

        Subgroups subgroups = new()
        {
            Items = FromOldList(old.Children, dataContext).ToList(),
        };

        if (old is DatabaseActivityLevelGroup levelOld)
        {
            GameLevel level = levelOld.Level;
            bool isStoryLevel = level.StoryId != 0;

            return new LevelSerializedActivityGroup
            {
                Type = "level",
                Timestamp = timestamp,
                Events = events,
                Subgroups = subgroups,
                LevelId = new SerializedLevelId
                {
                    LevelId = isStoryLevel ? level.StoryId : level.LevelId,
                    Type = level.SlotType.ToGameType(),
                },
            };
        }

        if (old is DatabaseActivityUserGroup userOld)
        {
            return new UserSerializedActivityGroup
            {
                Type = "user",
                Timestamp = timestamp,
                Events = events,
                Subgroups = subgroups,
                Username = userOld.User.Username,
            };
        }

        throw new NotImplementedException(old.GetType().Name);
    }

    public static IEnumerable<SerializedActivityGroup> FromOldList(IEnumerable<DatabaseActivityGroup> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}