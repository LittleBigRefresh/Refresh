namespace Refresh.GameServer.Types.Assets;

[Flags]
public enum AssetFlags
{
    None = 0,
    /// <summary>
    /// This asset can be dangerous to end users.
    /// </summary>
    Dangerous = 1 << 0,
    /// <summary>
    /// This asset is a media-type asset, e.g. a PNG or TEX.
    /// </summary>
    Media = 1 << 1,
    /// <summary>
    /// This asset will only ever be created by mods.
    /// </summary>
    Modded = 1 << 2,
}

public static class AssetSafetyLevelExtensions
{
    public static AssetFlags FromAssetType(GameAssetType type, GameSerializationMethod? method)
    {
        return type switch
        {
            // Non-binary levels will not be loadable 
            GameAssetType.Level => method == GameSerializationMethod.Binary
                ? AssetFlags.None
                : AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.StreamingLevelChunk => method == GameSerializationMethod.Binary
                ? AssetFlags.None
                : AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Plan => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.ThingRecording => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.SyncedProfile => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.GriefSongState => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.Quest => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.AdventureSharedData => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            GameAssetType.AdventureCreateProfile => method == GameSerializationMethod.Binary ? AssetFlags.None : AssetFlags.Modded,
            
            GameAssetType.VoiceRecording => method == GameSerializationMethod.Binary ? AssetFlags.Media : AssetFlags.Modded | AssetFlags.Media,
            GameAssetType.Painting => method == GameSerializationMethod.Binary ? AssetFlags.Media : AssetFlags.Modded | AssetFlags.Media,
            // Textures with the wrong serialization method will not be loadable
            GameAssetType.Texture => method == GameSerializationMethod.CompressedTexture
                ? AssetFlags.Media
                : AssetFlags.Dangerous | AssetFlags.Modded | AssetFlags.Media,
            GameAssetType.Jpeg => AssetFlags.Media,
            GameAssetType.Png => AssetFlags.Media,
            GameAssetType.Tga => AssetFlags.Media,
            GameAssetType.Mip => AssetFlags.Media,
            
            // Textures with the wrong serialization method will not be loadable
            GameAssetType.GameDataTexture => method == GameSerializationMethod.CompressedTexture
                ? AssetFlags.Modded | AssetFlags.Media
                : AssetFlags.Dangerous | AssetFlags.Media | AssetFlags.Modded,
            GameAssetType.AnimatedTexture => method == GameSerializationMethod.Binary
                ? AssetFlags.Modded | AssetFlags.Media
                : AssetFlags.Dangerous | AssetFlags.Modded | AssetFlags.Media,
            
            GameAssetType.GfxMaterial => AssetFlags.Modded,
            GameAssetType.Material => AssetFlags.Modded,
            GameAssetType.Mesh => AssetFlags.Modded,
            GameAssetType.Palette => AssetFlags.Modded,
            GameAssetType.SoftPhysicsSettings => AssetFlags.Modded,
            GameAssetType.Bevel => AssetFlags.Modded,
            GameAssetType.Animation => AssetFlags.Modded,
            GameAssetType.SettingsCharacter => AssetFlags.Modded,
            GameAssetType.Joint => AssetFlags.Modded,
            GameAssetType.MusicSettings => AssetFlags.Modded,
            GameAssetType.StaticMesh => AssetFlags.Modded,
            GameAssetType.PaintBrush => AssetFlags.Modded,
            
            GameAssetType.Script => AssetFlags.Dangerous | AssetFlags.Modded,
            
            GameAssetType.GuidSubstitution => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.DownloadableContent => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.GameConstants => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.CachedLevelData => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Game => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SettingsNetwork => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Packs => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.BigProfile => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SlotList => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.LocalProfile => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.LimitsSettings => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Tutorials => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.GuidList => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AudioMaterials => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Unknown => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.ReplayConfig => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Pins => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Instrument => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.OutfitList => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SkeletonMap => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SkeletonRegistry => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SkeletonAnimStyles => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AdventurePlayProfile => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.CachedCostumeData => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.DataLabels => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.TextureList => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.PoppetSettings => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.SettingsFluid => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.MixerSettings => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AnimationBank => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AnimationSet => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AnimationMap => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.AdventureMaps => AssetFlags.Dangerous | AssetFlags.Modded,
            GameAssetType.Fontface => AssetFlags.Dangerous | AssetFlags.Modded,
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}