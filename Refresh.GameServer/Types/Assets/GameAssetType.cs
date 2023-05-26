using Newtonsoft.Json;
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
}