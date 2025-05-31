using System.Xml.Serialization;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Activity.SerializedEvents;
using Refresh.Database.Models.Levels;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Activity.Groups;

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
            Items = SerializedEvent.FromOldList(old.Events, dataContext),
        };

        Subgroups subgroups = new()
        {
            Items = FromOldList(old.Children, dataContext),
        };

        if (old is DatabaseActivityLevelGroup levelOld)
        {
            return new LevelSerializedActivityGroup
            {
                Type = "level",
                Timestamp = timestamp,
                Events = events,
                Subgroups = subgroups,
                LevelId = new SerializedLevelId
                {
                    LevelId = levelOld.Level.LevelId,
                    Type = levelOld.Level.SlotType.ToGameType(),
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
    {
        throw new NotImplementedException();
    }
}