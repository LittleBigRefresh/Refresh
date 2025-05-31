using System.Xml.Serialization;
using Refresh.Database;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels;
using Refresh.Database.Query;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Activity.Groups;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Types.Activity;

[Serializable]
[XmlRoot("stream")]
[XmlType("stream")]
public class SerializedActivityPage : IDataConvertableFrom<SerializedActivityPage, DatabaseActivityPage>
{
    [XmlElement("start_timestamp")]
    public required long StartTimestamp { get; set; }
    
    [XmlElement("end_timestamp")]
    public required long EndTimestamp { get; set; }

    [XmlElement("groups")]
    public required SerializedActivityGroups Groups { get; set; }

    [XmlElement("users")]
    public required SerializedUserList Users { get; set; }
    
    [XmlElement("slots")]
    public required SerializedLevelList Levels { get; set; }

    public static SerializedActivityPage? FromOld(DatabaseActivityPage? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new SerializedActivityPage
        {
            StartTimestamp = 0,
            EndTimestamp = 0,
            Groups = new SerializedActivityGroups
            {
                Groups = SerializedActivityGroup.FromOldList(old.EventGroups, dataContext),
            },
            Users = new SerializedUserList
            {
                Users = GameUserResponse.FromOldList(old.Users, dataContext),
            },
            Levels = new SerializedLevelList
            {
                Items = GameLevelResponse.FromOldList(old.Levels, dataContext).ToList(),
            },
        };

    }

    public static IEnumerable<SerializedActivityPage> FromOldList(IEnumerable<DatabaseActivityPage> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}