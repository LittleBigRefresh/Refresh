using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.Game.Types.UserData;

[XmlRoot("npHandle")]
[XmlType("npHandle")]
public class SerializedUserHandle
{
    [XmlText] public string Username { get; set; } = string.Empty;

    [XmlAttribute("icon")] public string IconHash { get; set; } = "0";

    public static SerializedUserHandle FromUser(GameUser user, DataContext dataContext)
    {
        return new SerializedUserHandle
        {
            Username = user.Username,
            IconHash = dataContext.Database.GetAssetFromHash(user.IconHash)?.GetAsIcon(dataContext.Game, dataContext) ?? user.IconHash,
        };
    }
}