using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Assets;

[JsonObject(MemberSerialization.OptIn)]
public partial class GameAsset : IRealmObject
{
    [PrimaryKey] [JsonProperty] public string AssetHash { get; set; } = string.Empty;
    [JsonProperty] public GameUser? OriginalUploader { get; set; }
    [JsonProperty] public DateTimeOffset UploadDate { get; set; }
    [JsonProperty] public bool IsPSP { get; set; }
    [JsonProperty] [Ignored] public GameAssetType AssetType
    {
        get => (GameAssetType)this._AssetType;
        set => this._AssetType = (int)value;
    }

    // ReSharper disable once InconsistentNaming
    internal int _AssetType { get; set; }

    [JsonProperty] public IList<GameAsset> Dependencies { get; } = null!;

    [JsonProperty] [Ignored] public AssetSafetyLevel SafetyLevel => AssetSafetyLevelExtensions.FromAssetType(this.AssetType);
}