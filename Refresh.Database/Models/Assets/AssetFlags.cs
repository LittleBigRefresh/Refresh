namespace Refresh.Database.Models.Assets;

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
    public static AssetFlags FromAssetType(GameAssetType type, GameAssetFormat? method)
    {
        AssetFlags flags = type switch
        {
            // Common asset types created by the game
            GameAssetType.Level => AssetFlags.None,
            GameAssetType.StreamingLevelChunk => AssetFlags.None,
            GameAssetType.Plan => AssetFlags.None,
            GameAssetType.ThingRecording => AssetFlags.None,
            GameAssetType.SyncedProfile => AssetFlags.None,
            GameAssetType.GriefSongState => AssetFlags.None,
            GameAssetType.Quest => AssetFlags.None,
            GameAssetType.AdventureSharedData => AssetFlags.None,
            GameAssetType.AdventureCreateProfile => AssetFlags.None,
            GameAssetType.ChallengeGhost => AssetFlags.None,
            
            // Common media types created by the game
            GameAssetType.VoiceRecording => AssetFlags.Media,
            GameAssetType.Painting => AssetFlags.Media,
            GameAssetType.Texture => AssetFlags.Media,
            GameAssetType.Jpeg => AssetFlags.Media,
            GameAssetType.Png => AssetFlags.Media,
            GameAssetType.Tga => AssetFlags.Media,
            GameAssetType.Mip => AssetFlags.Media,
            
            // Uncommon, but still vanilla assets created by the game in niche scenarios 
            GameAssetType.GfxMaterial => AssetFlags.Media, // while not image/audio data like the other media types, this is marked as media because this file can contain full PS3 shaders
            GameAssetType.Material => AssetFlags.None,
            GameAssetType.Bevel => AssetFlags.None,
            
            // Modded media types
            GameAssetType.GameDataTexture => AssetFlags.Media | AssetFlags.Modded,
            GameAssetType.AnimatedTexture => AssetFlags.Media | AssetFlags.Modded,
            
            // Normal modded assets
            GameAssetType.Mesh => AssetFlags.Modded,
            GameAssetType.Palette => AssetFlags.Modded,
            GameAssetType.SoftPhysicsSettings => AssetFlags.Modded,
            GameAssetType.Animation => AssetFlags.Modded,
            GameAssetType.SettingsCharacter => AssetFlags.Modded,
            GameAssetType.Joint => AssetFlags.Modded,
            GameAssetType.MusicSettings => AssetFlags.Modded,
            GameAssetType.StaticMesh => AssetFlags.Modded,
            GameAssetType.PaintBrush => AssetFlags.Modded,
            
            // Dangerous modded assets
            GameAssetType.Script => AssetFlags.Dangerous | AssetFlags.Modded,
            
            // Asset types which have no reason to be referenced as hashed assets
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
        
        flags |= type switch {
            // Non-binary levels will not be loadable 
            GameAssetType.Level => method != GameAssetFormat.Binary ? AssetFlags.Dangerous : AssetFlags.None,
            GameAssetType.StreamingLevelChunk => method != GameAssetFormat.Binary ? AssetFlags.Dangerous : AssetFlags.None,
            // Textures with the wrong serialization method will not be loadable
            GameAssetType.Texture => method != GameAssetFormat.CompressedTexture ? AssetFlags.Dangerous : AssetFlags.None,
            GameAssetType.GameDataTexture => method != GameAssetFormat.CompressedTexture ? AssetFlags.Dangerous : AssetFlags.None,
            GameAssetType.AnimatedTexture => method != GameAssetFormat.CompressedTexture ? AssetFlags.Dangerous : AssetFlags.None,
            _ => AssetFlags.None,
        };
        
        return flags;
    }
}