using Newtonsoft.Json.Converters;

namespace Refresh.GameServer.Types.Assets;

[JsonConverter(typeof(StringEnumConverter))]
public enum GameAssetType
{
    Unknown = -1,
    /// <summary>
    /// A level, containing objects and such.
    /// </summary>
    /// <remarks>
    /// Magic: LVLb
    /// </remarks>
    Level = 0,
    /// <summary>
    /// An object, usually for prize bubbles or emitters and such.
    /// </summary>
    /// <remarks>
    /// Magic: PLNb
    /// </remarks>
    Plan = 1,
    /// <summary>
    /// An image, stored in a DDS-like format.
    /// </summary>
    /// <remarks>
    /// Magic: TEX(0x20)
    /// </remarks>
    Texture = 2,
    /// <summary>
    /// An image, stored in a JFIF container.
    /// </summary>
    /// <remarks>
    /// Magic: FF D8 FF EE
    /// </remarks>
    Jpeg = 3,
    /// <summary>
    /// An image, stored in a PNG container.
    /// </summary>
    /// <remarks>
    /// Magic: 89 50 4E 47 0D 0A 1A 0A
    /// </remarks>
    Png = 4,
    /// <summary>
    /// A material used for custom models.
    /// </summary>
    /// <remarks>
    /// Magic: GMTb
    /// </remarks>
    Material = 5,
    /// <summary>
    /// A mesh used for custom models.
    /// </summary>
    /// <remarks>
    /// Magic: MSHb
    /// </remarks>
    Mesh = 6,
    /// <summary>
    /// A <see cref="Texture"/>, with the header of GTF(0x20). Not sure of the difference but it seems to be safe.
    /// It is only seen in modded assets, not during normal gameplay.
    /// </summary>
    /// <remarks>
    /// Magic: GTF(0x20)
    /// </remarks>
    GameDataTexture = 7,
    /// <summary>
    /// A custom color palette, used for custom outfits.
    /// </summary>
    /// <remarks>
    /// Magic: PALb
    /// </remarks>
    Palette = 8,
    /// <summary>
    /// A script, programmed in a language named "fish fingers". Obviously, this can be dangerous.
    /// </summary>
    /// <remarks>
    /// Magic: FSHb
    /// </remarks>
    Script = 9,
    /// <summary>
    /// A recording of movement captured with a PlayStation Move controller.
    /// </summary>
    /// <remarks>
    /// Magic: RECb
    /// </remarks>
    MoveRecording = 10,
    /// <summary>
    /// A recording of audio, encoded using Speex.
    /// </summary>
    /// <remarks>
    /// Magic: VOPb
    /// </remarks>
    VoiceRecording = 11,
    /// <summary>
    /// A painting created with the move DLC
    /// </summary>
    /// <remarks>
    /// Magic: PTGb
    /// </remarks>
    Painting = 12,
}