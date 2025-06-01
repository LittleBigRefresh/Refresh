using System.Buffers.Binary;
using Newtonsoft.Json.Converters;

namespace Refresh.Database.Models.Assets;

[JsonConverter(typeof(StringEnumConverter))]
public enum GameAssetType
{
    Unknown = -1,
    
    /// <summary>
    /// An LBP texture, wraps over a DDS texture. This is sometimes uploaded by the game in normal operation.
    /// </summary>
    /// <remarks>
    /// Magic: "TEX"
    /// </remarks>
    Texture = 0,
    
    /// <summary>
    /// A type of texture generally stored only in the game files, this is only ever uploaded when mods are used.
    /// </summary>
    /// <remarks>
    /// Magic: "GTF"
    /// </remarks>
    GameDataTexture,
    
    /// <summary>
    /// A mesh for a custom model.
    /// </summary>
    /// <remarks>
    /// Magic: "MSH"
    /// </remarks>
    Mesh,
    
    /// <summary>
    /// An animation, used for custom costumes.
    /// </summary>
    /// <remarks>
    /// Magic: "ANM"
    /// </remarks>
    Animation,
    
    /// <remarks>
    /// Magic: "GSB"
    /// </remarks>
    GuidSubstitution,
    
    /// <summary>
    /// A material descriptor for custom materials, describes various render properties like the textures, shaders, reflection, etc.
    /// </summary>
    /// <remarks>
    /// Magic: "GMT"
    /// </remarks>
    GfxMaterial,
    
    /// <summary>
    /// A game level.
    /// </summary>
    /// <remarks>
    /// Magic: "LVL"
    /// </remarks>
    Level,
    
    /// <summary>
    /// A fish fingers script file, this has large control over the game, and can be very dangerous if left unmoderated.
    /// </summary>
    /// <remarks>
    /// Magic: "FSH"
    /// </remarks>
    Script,
    
    /// <remarks>
    /// Magic: "CHA"
    /// </remarks>
    SettingsCharacter,
    
    /// <summary>
    /// Defines the soft physics settings for a material.
    /// </summary>
    /// <remarks>
    /// Magic: "SSP"
    /// </remarks>
    SoftPhysicsSettings,
    
    /// <summary>
    /// A font face for the game.
    /// </summary>
    /// <remarks>
    /// Magic: "FNT"
    /// </remarks>
    Fontface,
    
    /// <summary>
    /// Describes the various physical properties of a material, such as friction, gravity, break force, etc.
    /// </summary>
    /// <remarks>
    /// Magic: "MAT"
    /// </remarks>
    Material,
    
    /// <remarks>
    /// Magic: "DLC"
    /// </remarks>
    DownloadableContent,
    
    /// <remarks>
    /// Magic: "JNT"
    /// </remarks>
    Joint,
    
    /// <remarks>
    /// Magic: "CON"
    /// </remarks>
    GameConstants,
    
    /// <remarks>
    /// Magic: "POP"
    /// </remarks>
    PoppetSettings,
    
    /// <remarks>
    /// Magic: "CLD"
    /// </remarks>
    CachedLevelData,
    
    /// <summary>
    /// Synced profile data. Stores the player and popit colors.
    /// </summary>
    /// <remarks>
    /// Magic: "PRF"
    /// This is bundled with Cross-Controller levels since the uploading logic for those levels isn't great.
    /// </remarks>
    SyncedProfile,
    
    /// <summary>
    /// Defines the bevel around a specific material.
    /// </summary>
    /// <remarks>
    /// Magic: "BEV"
    /// </remarks>
    Bevel,
    
    /// <summary>
    /// Defines the current game state, can be dumped by debug builds, and may be synced over P2P for divergent checking.
    /// </summary>
    /// <remarks>
    /// Magic: "GAM"
    /// </remarks>
    Game,
    
    /// <remarks>
    /// Magic: "NWS"
    /// </remarks>
    SettingsNetwork,
    
    /// <remarks>
    /// Magic: "PCK"
    /// </remarks>
    Packs,
    
    /// <remarks>
    /// Magic: "BPR"
    /// </remarks>
    BigProfile,
    
    /// <remarks>
    /// Magic: "SLT"
    /// </remarks>
    SlotList,
    
    /// <remarks>
    /// Magic: "ADC"
    /// </remarks>
    AdventureCreateProfile,
    
    /// <remarks>
    /// Magic: "IPR"
    /// </remarks>
    LocalProfile,
    
    /// <remarks>
    /// Magic: "LMT"
    /// </remarks>
    LimitsSettings,
    
    /// <remarks>
    /// Magic: "TUT"
    /// </remarks>
    Tutorials,
    
    /// <remarks>
    /// Magic: "GLT"
    /// </remarks>
    GuidList,
    
    /// <remarks>
    /// Magic: "AUM"
    /// </remarks>
    AudioMaterials,
    
    /// <remarks>
    /// Magic: "SSF"
    /// </remarks>
    SettingsFluid,
    
    /// <summary>
    /// The "plan" for how to build an object, contains all the "Thing"s attached to it, such as stickers, models, scripts, etc.
    /// </summary>
    /// <remarks>
    /// Magic: "PLN"
    /// </remarks>
    Plan,
    
    /// <summary>
    /// This asset does nothing, but is still defined to exist.
    /// </summary>
    /// <remarks>
    /// Magic: "TXL"
    /// </remarks>
    TextureList,
    
    /// <remarks>
    /// Magic: "MUS"
    /// </remarks>
    MusicSettings,
    
    /// <remarks>
    /// Magic: "MIX"
    /// </remarks>
    MixerSettings,
    
    /// <remarks>
    /// Magic: "REP"
    /// </remarks>
    ReplayConfig,
    
    /// <summary>
    /// A custom color palette, used for custom outfits.
    /// </summary>
    /// <remarks>
    /// Magic: "PAL"
    /// </remarks>
    Palette,
    
    /// <remarks>
    /// Magic: "SMH"
    /// </remarks>
    StaticMesh,
    
    /// <remarks>
    /// Magic: "ATX"
    /// </remarks>
    AnimatedTexture,
    
    /// <summary>
    /// A recording of audio, encoded using Speex.
    /// </summary>
    /// <remarks>
    /// Magic: "VOP"
    /// </remarks>
    VoiceRecording,
    
    /// <remarks>
    /// Magic: "PIN"
    /// </remarks>
    Pins,
    
    /// <remarks>
    /// Magic: "INS"
    /// </remarks>
    Instrument,
    
    /// <remarks>
    /// Magic: "OFT"
    /// </remarks>
    OutfitList,
    
    /// <remarks>
    /// Magic: "PBR"
    /// </remarks>
    PaintBrush,
    
    /// <summary>
    /// A recording of movement captured with a PlayStation Move controller.
    /// </summary>
    /// <remarks>
    /// Magic: "REC"
    /// </remarks>
    ThingRecording,
    
    /// <summary>
    /// A painting created with the move DLC
    /// </summary>
    /// <remarks>
    /// Magic: "PTG"
    /// </remarks>
    Painting,
    
    /// <remarks>
    /// Magic: "QST"
    /// </remarks>
    Quest,
    
    /// <remarks>
    /// Magic: "ABK"
    /// </remarks>
    AnimationBank,
    
    /// <remarks>
    /// Magic: "AST"
    /// </remarks>
    AnimationSet,
    
    /// <remarks>
    /// Magic: "SMP"
    /// </remarks>
    SkeletonMap,
    
    /// <remarks>
    /// Magic: "SRG"
    /// </remarks>
    SkeletonRegistry,
    
    /// <remarks>
    /// Magic: "SAS"
    /// </remarks>
    SkeletonAnimStyles,
    
    /// <summary>
    /// A chunk of a level, containing objects and such.
    /// Used for 'Dynamic Thermometer' levels in LBP3.
    /// </summary>
    /// <remarks>
    /// Magic: "CHK"
    /// </remarks>
    StreamingLevelChunk,
    
    /// <remarks>
    /// Magic: "ADS"
    /// </remarks>
    AdventureSharedData,
    
    /// <remarks>
    /// Magic: "ADP"
    /// </remarks>
    AdventurePlayProfile,
    
    /// <remarks>
    /// Magic: "AMP"
    /// </remarks>
    AnimationMap,
    
    /// <remarks>
    /// Magic: "CCD"
    /// </remarks>
    CachedCostumeData,
    
    /// <remarks>
    /// Magic: "DLA"
    /// </remarks>
    DataLabels,
    
    /// <remarks>
    /// Magic: "ADM"
    /// </remarks>
    AdventureMaps,
    
    /// <summary>
    /// A PSP MIP image file. While this file type has no magic, we do some heuristics on the file to detect it.
    /// This image is uploaded by LBP PSP for new levels, and is what it loads for level badges.
    /// </summary>
    /// <seealso cref="Refresh.GameServer.Importing.Importer.IsMip"/>
    Mip,
    
    /// <summary>
    /// A file containing information about the currently playing song, uploaded during a grief report.
    /// </summary>
    GriefSongState,
    
    /// <summary>
    /// An image, stored in a JFIF container.
    /// </summary>
    /// <remarks>
    /// Magic: FF D8 FF EE
    /// </remarks>
    Jpeg,
    
    /// <summary>
    /// An image, stored in a PNG container.
    /// </summary>
    /// <remarks>
    /// Magic: 89 50 4E 47 0D 0A 1A 0A
    /// </remarks>
    Png,
    
    /// <summary>
    /// An image, stored in a TGA container. While this file type has no magic, we do some heuristics on the file to detect it.
    /// </summary>
    /// <seealso cref="Refresh.GameServer.Importing.Importer.IsPspTga"/>
    Tga,

    /// <summary>
    /// LBP Hub ghost data, sent as XML.
    /// </summary>
    /// <remarks>
    /// Magic: Opening tag for the root element, "ghost"
    /// </remarks>
    ChallengeGhost,
}

public static class GameAssetTypeExtensions
{
    private static readonly Dictionary<int, GameAssetType> LbpMagics = new()
    {
        { BinaryPrimitives.ReadInt32BigEndian("TEX\0"u8), GameAssetType.Texture },
        { BinaryPrimitives.ReadInt32BigEndian("GTF\0"u8), GameAssetType.GameDataTexture },
        { BinaryPrimitives.ReadInt32BigEndian("MSH\0"u8), GameAssetType.Mesh },
        { BinaryPrimitives.ReadInt32BigEndian("ANM\0"u8), GameAssetType.Animation },
        { BinaryPrimitives.ReadInt32BigEndian("GSB\0"u8), GameAssetType.GuidSubstitution },
        { BinaryPrimitives.ReadInt32BigEndian("GMT\0"u8), GameAssetType.GfxMaterial },
        { BinaryPrimitives.ReadInt32BigEndian("LVL\0"u8), GameAssetType.Level },
        { BinaryPrimitives.ReadInt32BigEndian("FSH\0"u8), GameAssetType.Script },
        { BinaryPrimitives.ReadInt32BigEndian("CHA\0"u8), GameAssetType.SettingsCharacter },
        { BinaryPrimitives.ReadInt32BigEndian("SSP\0"u8), GameAssetType.SoftPhysicsSettings },
        { BinaryPrimitives.ReadInt32BigEndian("FNT\0"u8), GameAssetType.Fontface },
        { BinaryPrimitives.ReadInt32BigEndian("MAT\0"u8), GameAssetType.Material },
        { BinaryPrimitives.ReadInt32BigEndian("DLC\0"u8), GameAssetType.DownloadableContent },
        { BinaryPrimitives.ReadInt32BigEndian("JNT\0"u8), GameAssetType.Joint },
        { BinaryPrimitives.ReadInt32BigEndian("CON\0"u8), GameAssetType.GameConstants },
        { BinaryPrimitives.ReadInt32BigEndian("POP\0"u8), GameAssetType.PoppetSettings },
        { BinaryPrimitives.ReadInt32BigEndian("CLD\0"u8), GameAssetType.CachedLevelData },
        { BinaryPrimitives.ReadInt32BigEndian("PRF\0"u8), GameAssetType.SyncedProfile },
        { BinaryPrimitives.ReadInt32BigEndian("BEV\0"u8), GameAssetType.Bevel },
        { BinaryPrimitives.ReadInt32BigEndian("GAM\0"u8), GameAssetType.Game },
        { BinaryPrimitives.ReadInt32BigEndian("NWS\0"u8), GameAssetType.SettingsNetwork },
        { BinaryPrimitives.ReadInt32BigEndian("PCK\0"u8), GameAssetType.Packs },
        { BinaryPrimitives.ReadInt32BigEndian("BPR\0"u8), GameAssetType.BigProfile },
        { BinaryPrimitives.ReadInt32BigEndian("SLT\0"u8), GameAssetType.SlotList },
        { BinaryPrimitives.ReadInt32BigEndian("ADC\0"u8), GameAssetType.AdventureCreateProfile },
        { BinaryPrimitives.ReadInt32BigEndian("IPR\0"u8), GameAssetType.LocalProfile },
        { BinaryPrimitives.ReadInt32BigEndian("LMT\0"u8), GameAssetType.LimitsSettings },
        { BinaryPrimitives.ReadInt32BigEndian("TUT\0"u8), GameAssetType.Tutorials },
        { BinaryPrimitives.ReadInt32BigEndian("GLT\0"u8), GameAssetType.GuidList },
        { BinaryPrimitives.ReadInt32BigEndian("AUM\0"u8), GameAssetType.AudioMaterials },
        { BinaryPrimitives.ReadInt32BigEndian("SSF\0"u8), GameAssetType.SettingsFluid },
        { BinaryPrimitives.ReadInt32BigEndian("PLN\0"u8), GameAssetType.Plan },
        { BinaryPrimitives.ReadInt32BigEndian("TXL\0"u8), GameAssetType.TextureList },
        { BinaryPrimitives.ReadInt32BigEndian("MUS\0"u8), GameAssetType.MusicSettings },
        { BinaryPrimitives.ReadInt32BigEndian("MIX\0"u8), GameAssetType.MixerSettings },
        { BinaryPrimitives.ReadInt32BigEndian("REP\0"u8), GameAssetType.ReplayConfig },
        { BinaryPrimitives.ReadInt32BigEndian("PAL\0"u8), GameAssetType.Palette },
        { BinaryPrimitives.ReadInt32BigEndian("SMH\0"u8), GameAssetType.StaticMesh },
        { BinaryPrimitives.ReadInt32BigEndian("ATX\0"u8), GameAssetType.AnimatedTexture },
        { BinaryPrimitives.ReadInt32BigEndian("VOP\0"u8), GameAssetType.VoiceRecording },
        { BinaryPrimitives.ReadInt32BigEndian("PIN\0"u8), GameAssetType.Pins },
        { BinaryPrimitives.ReadInt32BigEndian("INS\0"u8), GameAssetType.Instrument },
        { BinaryPrimitives.ReadInt32BigEndian("OFT\0"u8), GameAssetType.OutfitList },
        { BinaryPrimitives.ReadInt32BigEndian("PBR\0"u8), GameAssetType.PaintBrush },
        { BinaryPrimitives.ReadInt32BigEndian("REC\0"u8), GameAssetType.ThingRecording },
        { BinaryPrimitives.ReadInt32BigEndian("PTG\0"u8), GameAssetType.Painting },
        { BinaryPrimitives.ReadInt32BigEndian("QST\0"u8), GameAssetType.Quest },
        { BinaryPrimitives.ReadInt32BigEndian("ABK\0"u8), GameAssetType.AnimationBank },
        { BinaryPrimitives.ReadInt32BigEndian("AST\0"u8), GameAssetType.AnimationSet },
        { BinaryPrimitives.ReadInt32BigEndian("SMP\0"u8), GameAssetType.SkeletonMap },
        { BinaryPrimitives.ReadInt32BigEndian("SRG\0"u8), GameAssetType.SkeletonRegistry },
        { BinaryPrimitives.ReadInt32BigEndian("SAS\0"u8), GameAssetType.SkeletonAnimStyles },
        { BinaryPrimitives.ReadInt32BigEndian("CHK\0"u8), GameAssetType.StreamingLevelChunk },
        { BinaryPrimitives.ReadInt32BigEndian("ADS\0"u8), GameAssetType.AdventureSharedData },
        { BinaryPrimitives.ReadInt32BigEndian("ADP\0"u8), GameAssetType.AdventurePlayProfile },
        { BinaryPrimitives.ReadInt32BigEndian("AMP\0"u8), GameAssetType.AnimationMap },
        { BinaryPrimitives.ReadInt32BigEndian("CCD\0"u8), GameAssetType.CachedCostumeData },
        { BinaryPrimitives.ReadInt32BigEndian("DLA\0"u8), GameAssetType.DataLabels },
        { BinaryPrimitives.ReadInt32BigEndian("ADM\0"u8), GameAssetType.AdventureMaps },
    };
    
    public static GameAssetType? LbpMagicToGameAssetType(ReadOnlySpan<byte> magic)
    {
        int magicInt = magic[0] << 24 | magic[1] << 16 | magic[2] << 8;
        
        if (!LbpMagics.TryGetValue(magicInt, out GameAssetType assetType))
            return null;
        
        return assetType;
    }
    
    public static string? LbpMagic(this GameAssetType type)
    {
        return type switch
        {
            GameAssetType.Texture => "TEX",
            GameAssetType.GameDataTexture => "GTF",
            GameAssetType.Mesh => "MSH",
            GameAssetType.Animation => "ANM",
            GameAssetType.GuidSubstitution => "GSB",
            GameAssetType.GfxMaterial => "GMT",
            GameAssetType.Level => "LVL",
            GameAssetType.Script => "FSH",
            GameAssetType.SettingsCharacter => "CHA",
            GameAssetType.SoftPhysicsSettings => "SSP",
            GameAssetType.Fontface => "FNT",
            GameAssetType.Material => "MAT",
            GameAssetType.DownloadableContent => "DLC",
            GameAssetType.Joint => "JNT",
            GameAssetType.GameConstants => "CON",
            GameAssetType.PoppetSettings => "POP",
            GameAssetType.CachedLevelData => "CLD",
            GameAssetType.SyncedProfile => "PRF",
            GameAssetType.Bevel => "BEV",
            GameAssetType.Game => "GAM",
            GameAssetType.SettingsNetwork => "NWS",
            GameAssetType.Packs => "PCK",
            GameAssetType.BigProfile => "BPR",
            GameAssetType.SlotList => "SLT",
            GameAssetType.AdventureCreateProfile => "ADC",
            GameAssetType.LocalProfile => "IPR",
            GameAssetType.LimitsSettings => "LMT",
            GameAssetType.Tutorials => "TUT",
            GameAssetType.GuidList => "GLT",
            GameAssetType.AudioMaterials => "AUM",
            GameAssetType.SettingsFluid => "SSF",
            GameAssetType.Plan => "PLN",
            GameAssetType.TextureList => "TXL",
            GameAssetType.MusicSettings => "MUS",
            GameAssetType.MixerSettings => "MIX",
            GameAssetType.ReplayConfig => "REP",
            GameAssetType.Palette => "PAL",
            GameAssetType.StaticMesh => "SMH",
            GameAssetType.AnimatedTexture => "ATX",
            GameAssetType.VoiceRecording => "VOP",
            GameAssetType.Pins => "PIN",
            GameAssetType.Instrument => "INS",
            GameAssetType.OutfitList => "OFT",
            GameAssetType.PaintBrush => "PBR",
            GameAssetType.ThingRecording => "REC",
            GameAssetType.Painting => "PTG",
            GameAssetType.Quest => "QST",
            GameAssetType.AnimationBank => "ABK",
            GameAssetType.AnimationSet => "AST",
            GameAssetType.SkeletonMap => "SMP",
            GameAssetType.SkeletonRegistry => "SRG",
            GameAssetType.SkeletonAnimStyles => "SAS",
            GameAssetType.StreamingLevelChunk => "CHK",
            GameAssetType.AdventureSharedData => "ADS",
            GameAssetType.AdventurePlayProfile => "ADP",
            GameAssetType.AnimationMap => "AMP",
            GameAssetType.CachedCostumeData => "CCD",
            GameAssetType.DataLabels => "DLA",
            GameAssetType.AdventureMaps => "ADM",
            GameAssetType.GriefSongState => "MATT",
            GameAssetType.ChallengeGhost => "<ghost>",
            _ => null,
        };
    }
}