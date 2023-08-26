using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Bunkum.HttpServer;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public abstract class Importer
{
    private readonly LoggerContainer<BunkumContext> _logger;
    protected readonly Stopwatch Stopwatch;

    protected Importer(LoggerContainer<BunkumContext>? logger = null)
    {
        if (logger == null)
        {
            logger = new LoggerContainer<BunkumContext>();
            logger.RegisterLogger(new ConsoleLogger());
        }

        this._logger = logger;
        this.Stopwatch = new Stopwatch();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Info(string message)
    {
        this._logger.LogInfo(BunkumContext.UserContent, $"[{this.Stopwatch.ElapsedMilliseconds}ms] {message}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Warn(string message)
    {
        this._logger.LogWarning(BunkumContext.UserContent, $"[{this.Stopwatch.ElapsedMilliseconds}ms] {message}");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesMagic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> magic)
    {
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
    
    protected GameAssetType DetermineAssetType(ReadOnlySpan<byte> data)
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
        
        // Traditional files
        // Good reference for magics: https://en.wikipedia.org/wiki/List_of_file_signatures
        if (MatchesMagic(data, 0xFFD8FFE0)) return GameAssetType.Jpeg;
        if (MatchesMagic(data, 0x89504E470D0A1A0A)) return GameAssetType.Png;
        
        this.Warn($"Unknown asset header [0x{Convert.ToHexString(data[..4])}] [str: {Encoding.ASCII.GetString(data[..4])}]");

        return GameAssetType.Unknown;
    }
}