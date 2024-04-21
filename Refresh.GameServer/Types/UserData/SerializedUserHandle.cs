using System.Xml.Serialization;
using Bunkum.Core.Storage;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("npHandle")]
[XmlType("npHandle")]
public class SerializedUserHandle
{
    [XmlText] public string? Username { get; set; } = string.Empty;

    [XmlAttribute("icon")] public string IconHash { get; set; } = "0";

    public static SerializedUserHandle FromUser(GameUser user) =>
        new()
        {
            Username = user.Username,
            IconHash = user.IconHash,
        };

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore, TokenGame game)
    {
        //If the icon is a remote asset
        if(!this.IconHash.StartsWith('g'))
            //Get the icon form of that remote asset
            this.IconHash = database.GetAssetFromHash(this.IconHash)?.GetAsIcon(game, database, dataStore) ?? this.IconHash;
    }
}