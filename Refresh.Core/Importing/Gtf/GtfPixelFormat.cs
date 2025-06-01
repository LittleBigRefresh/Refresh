namespace Refresh.Core.Importing.Gtf;

//See https://github.com/RPCS3/rpcs3/blob/23cb67e0a10f7d7ecf39122fa697af8dcc30690e/rpcs3/Emu/RSX/gcm_enums.h#L135
public enum GtfPixelFormat : byte
{
    //Color flag
    B8 = 0x81,
    A1R5G5B5 = 0x82,
    A4R4G4B4 = 0x83,
    R5G6B5 = 0x84,
    A8R8G8B8 = 0x85,
    CompressedDxt1 = 0x86,
    CompressedDxt23 = 0x87,
    CompressedDxt45 = 0x88,
    G8B8 = 0x8B,
    CompressedB8R8G8R8 = 0x8D, // NOTE: 0xAD in firmware
    CompressedR8B8R8G8 = 0x8E, // NOTE: 0xAE in firmware
    R6G5B5 = 0x8F,
    Depth24D8 = 0x90,
    Depth24D8Float = 0x91,
    Depth16 = 0x92,
    Depth16Float = 0x93,
    X16 = 0x94,
    Y16X16 = 0x95,
    R5G5B5A1 = 0x97,
    CompressedHiLo8 = 0x98,
    CompressedHiLoS8 = 0x99,
    W16Z16Y16X16Float = 0x9A,
    W32Z32Y32X32Float = 0x9B,
    X32Float = 0x9C,
    D1R5G5B5 = 0x9D,
    D8R8G8B8 = 0x9E,
    Y16X16Float = 0x9F,
    //Swizzle flag
    SZ = 0x00,
    LN = 0x20,
    // Normalization Flag
    NR = 0x00,
    UN = 0x40,
}

public static class GtfPixelFormatExtensions
{
    public static int BitsPerPixel(this GtfPixelFormat pixelFormat)
    {
        return pixelFormat switch {
            GtfPixelFormat.B8 => 8,
            GtfPixelFormat.A1R5G5B5 => 16,
            GtfPixelFormat.A4R4G4B4 => 16,
            GtfPixelFormat.R5G6B5 => 16,
            GtfPixelFormat.A8R8G8B8 => 32,
            GtfPixelFormat.CompressedDxt1 => 32,
            GtfPixelFormat.CompressedDxt23 => 32,
            GtfPixelFormat.CompressedDxt45 => 32,
            GtfPixelFormat.G8B8 => 16,
            GtfPixelFormat.CompressedB8R8G8R8 => 32,
            GtfPixelFormat.CompressedR8B8R8G8 => 32,
            GtfPixelFormat.R6G5B5 => 16,
            GtfPixelFormat.Depth24D8 => 32,
            GtfPixelFormat.Depth24D8Float => 32,
            GtfPixelFormat.Depth16 => 16,
            GtfPixelFormat.Depth16Float => 16,
            GtfPixelFormat.X16 => 16,
            GtfPixelFormat.Y16X16 => 32,
            GtfPixelFormat.R5G5B5A1 => 16,
            GtfPixelFormat.CompressedHiLo8 => 16,
            GtfPixelFormat.CompressedHiLoS8 => 16,
            GtfPixelFormat.W16Z16Y16X16Float => 64,
            GtfPixelFormat.W32Z32Y32X32Float => 128,
            GtfPixelFormat.X32Float => 32,
            GtfPixelFormat.D1R5G5B5 => 16,
            GtfPixelFormat.D8R8G8B8 => 32,
            GtfPixelFormat.Y16X16Float => 32,
            _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null)
        };
    }
}