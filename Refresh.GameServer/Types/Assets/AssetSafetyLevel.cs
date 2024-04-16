namespace Refresh.GameServer.Types.Assets;

public enum AssetSafetyLevel
{
    /// <summary>
    /// This asset is used in normal gameplay/operation and is okay to be uploaded.
    /// </summary>
    /// <seealso cref="SafeMedia"/>
    Safe = 0,
    /// <summary>
    /// This asset is still used in normal gameplay similar to <see cref="Safe"/>, and is okay to be uploaded, but the distinction can be useful.
    /// </summary>
    SafeMedia = 1,
    /// <summary>
    /// This asset may disrupt the "vanilla" feel of a server and it's content, but is otherwise harmless.
    /// </summary>
    PotentiallyUnwanted = 2,
    /// <summary>
    /// This asset may cause harm if uploaded.
    /// </summary>
    Dangerous = 3,
}

public static class AssetSafetyLevelExtensions
{
    public static AssetSafetyLevel FromAssetType(GameAssetType type)
    {
        return type switch
        {
            GameAssetType.Level => AssetSafetyLevel.Safe,
            GameAssetType.LevelChunk => AssetSafetyLevel.Safe,
            GameAssetType.Plan => AssetSafetyLevel.Safe,
            GameAssetType.MoveRecording => AssetSafetyLevel.Safe,
            GameAssetType.SyncedProfile => AssetSafetyLevel.Safe,
            GameAssetType.GriefSongState => AssetSafetyLevel.Safe,
            
            GameAssetType.VoiceRecording => AssetSafetyLevel.SafeMedia,
            GameAssetType.Painting => AssetSafetyLevel.SafeMedia,
            GameAssetType.Texture => AssetSafetyLevel.SafeMedia,
            GameAssetType.Jpeg => AssetSafetyLevel.SafeMedia,
            GameAssetType.Png => AssetSafetyLevel.SafeMedia,
            GameAssetType.Tga => AssetSafetyLevel.SafeMedia,
            GameAssetType.Mip => AssetSafetyLevel.SafeMedia,
            
            GameAssetType.GfxMaterial => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Material => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Mesh => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.GameDataTexture => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Palette => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.SoftPhysicsSettings => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Bevel => AssetSafetyLevel.PotentiallyUnwanted,
            
            GameAssetType.Script => AssetSafetyLevel.Dangerous,
            GameAssetType.Unknown => AssetSafetyLevel.Dangerous,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}