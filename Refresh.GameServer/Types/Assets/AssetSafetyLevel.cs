namespace Refresh.GameServer.Types.Assets;

public enum AssetSafetyLevel
{
    /// <summary>
    /// This asset is used in normal gameplay/operation and is okay to be uploaded.
    /// </summary>
    Safe = 0,
    /// <summary>
    /// This asset may disrupt the "vanilla" feel of a server and it's content, but is otherwise harmless.
    /// </summary>
    PotentiallyUnwanted = 1,
    /// <summary>
    /// This asset may cause harm if uploaded.
    /// </summary>
    Dangerous = 2,
}

public static class AssetSafetyLevelExtensions
{
    public static AssetSafetyLevel FromAssetType(GameAssetType type)
    {
        return type switch
        {
            GameAssetType.Level => AssetSafetyLevel.Safe,
            GameAssetType.Plan => AssetSafetyLevel.Safe,
            GameAssetType.Texture => AssetSafetyLevel.Safe,
            GameAssetType.Jpeg => AssetSafetyLevel.Safe,
            GameAssetType.Png => AssetSafetyLevel.Safe,
            GameAssetType.MoveRecording => AssetSafetyLevel.Safe,
            GameAssetType.VoiceRecording => AssetSafetyLevel.Safe,
            GameAssetType.Painting => AssetSafetyLevel.Safe,
            
            GameAssetType.Material => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Mesh => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.GameDataTexture => AssetSafetyLevel.PotentiallyUnwanted,
            GameAssetType.Palette => AssetSafetyLevel.PotentiallyUnwanted,
            
            GameAssetType.Script => AssetSafetyLevel.Dangerous,
            GameAssetType.Unknown => AssetSafetyLevel.Dangerous,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}