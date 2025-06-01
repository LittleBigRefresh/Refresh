using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Activity;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Activity.Groups;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Types.Activity;

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
            StartTimestamp = old.Start.ToUnixTimeMilliseconds(),
            EndTimestamp = old.End.ToUnixTimeMilliseconds(),
            Groups = new SerializedActivityGroups
            {
                Groups = SerializedActivityGroup.FromOldList(old.EventGroups, dataContext).ToList(),
            },
            Users = new SerializedUserList
            {
                Users = GameUserResponse.FromOldList(old.Users, dataContext).ToList(),
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