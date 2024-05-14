using System.Diagnostics;
using System.Xml.Serialization;
using Bunkum.Core.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("npHandle")]
[XmlType("npHandle")]
public class SerializedUserHandle
{
    [XmlText] public string Username { get; set; } = string.Empty;

    [XmlAttribute("icon")] public string IconHash { get; set; } = "0";

    public static SerializedUserHandle FromUser(GameUser user, DataContext dataContext)
    {
        TokenGame game = dataContext.Game ?? TokenGame.Website;
        
        return new SerializedUserHandle
        {
            Username = user.Username,
            IconHash = dataContext.Database.GetAssetFromHash(user.IconHash)?.GetAsIcon(game, dataContext) ?? user.IconHash,
        };
    }
}