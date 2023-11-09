using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Assets;

public partial class GameAsset : IRealmObject
{
    [PrimaryKey] public string AssetHash { get; set; } = string.Empty;
    public GameUser? OriginalUploader { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public bool IsPSP { get; set; }
    public int SizeInBytes { get; set; }
    [Ignored] public GameAssetType AssetType
    {
        get => (GameAssetType)this._AssetType;
        set => this._AssetType = (int)value;
    }

    // ReSharper disable once InconsistentNaming
    internal int _AssetType { get; set; }

    public IList<string> Dependencies { get; } = null!;

    [Ignored] public AssetSafetyLevel SafetyLevel => AssetSafetyLevelExtensions.FromAssetType(this.AssetType);
}