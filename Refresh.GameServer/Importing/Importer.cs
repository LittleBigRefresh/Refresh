using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Bunkum.Core;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public abstract class Importer
{
    private readonly Logger _logger;
    protected readonly Stopwatch Stopwatch;

    protected Importer(Logger? logger = null)
    {
        if (logger == null)
        {
            logger = new Logger();
        }

        this._logger = logger;
        this.Stopwatch = new Stopwatch();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Info(string message)
    {
        this._logger.LogInfo(BunkumCategory.UserContent, $"[{this.Stopwatch.ElapsedMilliseconds}ms] {message}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Warn(string message)
    {
        this._logger.LogWarning(BunkumCategory.UserContent, $"[{this.Stopwatch.ElapsedMilliseconds}ms] {message}");
    }

    protected void Debug(string message)
    {
        this._logger.LogDebug(BunkumCategory.UserContent, $"[{this.Stopwatch.ElapsedMilliseconds}ms] {message}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> magic)
    {
        if (magic.Length > data.Length) return false;
        return data[..magic.Length].SequenceEqual(magic);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, uint magic)
    {
        Span<byte> magicSpan = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(magicSpan, BinaryPrimitives.ReverseEndianness(magic));
        return MatchesMagic(data, magicSpan);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, ulong magic)
    {
        Span<byte> magicSpan = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(magicSpan, BinaryPrimitives.ReverseEndianness(magic));
        return MatchesMagic(data, magicSpan);
    }

    /// <summary>
    /// Tries to detect TGA files sent from the PSP
    /// </summary>
    /// <param name="data">The data to check</param>
    /// <returns>Whether the file is likely of TGA format</returns>
    private static bool IsPspTga(ReadOnlySpan<byte> data)
    {
        byte imageIdLength = data[0];
        byte colorMapType = data[1];
        byte imageType = data[2];
        ReadOnlySpan<byte> colorMapSpecification = data[3..8];
        ReadOnlySpan<byte> imageSpecification = data[8..18];
        short xOrigin = BinaryPrimitives.ReadInt16LittleEndian(imageSpecification[..2]);
        short yOrigin = BinaryPrimitives.ReadInt16LittleEndian(imageSpecification[2..4]);
        ushort width = BinaryPrimitives.ReadUInt16LittleEndian(imageSpecification[4..6]);
        ushort height = BinaryPrimitives.ReadUInt16LittleEndian(imageSpecification[6..8]);
        byte depth = imageSpecification[8];
        byte descriptor = imageSpecification[9];

        //PSP does not seem to fill out this information
        if (imageIdLength != 0) return false;
        if (xOrigin != 0) return false;
        if (yOrigin != 0) return false;
        //These are the fields set by PSP, that shouldn't change from image to image
        if (colorMapType != 1) return false;
        if (descriptor != 0) return false;
        if (imageType != 1) return false;
        if (depth != 8) return false;
        //Reasonable validation checks (PSP seems to only send images of max size 480x272)
        if (width > 500) return false;
        if (height > 300) return false;
        
        return true;
    }
    
    protected GameAssetType DetermineAssetType(ReadOnlySpan<byte> data, TokenPlatform? tokenPlatform)
    {
        // LBP assets
        if (MatchesMagic(data, "TEX "u8)) return GameAssetType.Texture;
        if (MatchesMagic(data, "GTF "u8)) return GameAssetType.GameDataTexture;
        if (MatchesMagic(data, "PLNb"u8)) return GameAssetType.Plan;
        if (MatchesMagic(data, "LVLb"u8)) return GameAssetType.Level;
        if (MatchesMagic(data, "GMTb"u8)) return GameAssetType.Material;
        if (MatchesMagic(data, "MSHb"u8)) return GameAssetType.Mesh;
        if (MatchesMagic(data, "PALb"u8)) return GameAssetType.Palette;
        if (MatchesMagic(data, "FSHb"u8)) return GameAssetType.Script;
        if (MatchesMagic(data, "RECb"u8)) return GameAssetType.MoveRecording;
        if (MatchesMagic(data, "VOPb"u8)) return GameAssetType.VoiceRecording;
        if (MatchesMagic(data, "PTGb"u8)) return GameAssetType.Painting;
        if (MatchesMagic(data, "PRFb"u8)) return GameAssetType.SyncedProfile;
        
        // Traditional files
        // Good reference for magics: https://en.wikipedia.org/wiki/List_of_file_signatures
        if (MatchesMagic(data, 0xFFD8FFE0)) return GameAssetType.Jpeg;
        if (MatchesMagic(data, 0x89504E470D0A1A0A)) return GameAssetType.Png;

        if (tokenPlatform is null or TokenPlatform.PSP && IsPspTga(data)) return GameAssetType.Tga;
        
        this.Warn($"Unknown asset header [0x{Convert.ToHexString(data[..4])}] [str: {Encoding.ASCII.GetString(data[..4])}]");

        return GameAssetType.Unknown;
    }
}