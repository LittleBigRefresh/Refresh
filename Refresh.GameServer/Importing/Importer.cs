using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Bunkum.Core;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Resources;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Importing;

public abstract class Importer
{
    private readonly Logger _logger;
    protected readonly Stopwatch Stopwatch;
    
    public static Lazy<byte[]?> PSPKey = null!;

    protected Importer(Logger? logger = null)
    {
        if (logger == null)
        {
            logger = new Logger();
        }

        this._logger = logger;
        this.Stopwatch = new Stopwatch();

        PSPKey = new(() =>
        {
            try
            {
                //Read the key
                byte[] key = File.ReadAllBytes("keys/psp");
                
                //If the hash matches, return the read key
                if (SHA1.HashData(key).AsSpan().SequenceEqual(new byte[] { 0x12, 0xb5, 0xa8, 0xb5, 0x91, 0x55, 0x24, 0x96, 0x00, 0xdf, 0x0e, 0x33, 0xf9, 0xc5, 0xa8, 0x76, 0xc1, 0x85, 0x43, 0xfe })) 
                    return key;
                
                //If the hash does not match, log an error and return null
                this._logger.LogError(BunkumCategory.Digest, "PSP key failed to validate! Correct hash is 12b5a8b59155249600df0e33f9c5a876c18543fe");
                return null;
            }
            catch(Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException) || e.GetType() == typeof(DirectoryNotFoundException))
                {
                    this._logger.LogWarning(BunkumCategory.Digest, "PSP key file not found, encrypting/decrypting of PSP images will not work.");
                }
                else
                {
                    this._logger.LogError(BunkumCategory.Digest, "Unknown error while loading PSP key! err: {0}", e.ToString());
                }
                
                return null;
            }
        },  LazyThreadSafetyMode.ExecutionAndPublication);
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

    private bool IsMip(Span<byte> rawData)
    {
        //If we dont have a key, then we cant determine the data type
        if (PSPKey.Value == null) return false;

        //Data less than this size isn't encrypted(?) and all Mip files uploaded to the server will be encrypted
        //See https://github.com/ennuo/lbparc/blob/16ad36aa7f4eae2f7b406829e604082750f16fe1/tools/toggle.js#L33
        if (rawData.Length < 0x19) return false;

        try
        {
            //Decrypt the data
            ReadOnlySpan<byte> data = ResourceHelper.PspDecrypt(rawData.ToArray(), PSPKey.Value);
            
            uint clutOffset = BinaryPrimitives.ReadUInt32LittleEndian(data[..4]);
            uint width = BinaryPrimitives.ReadUInt32LittleEndian(data[4..8]);
            uint height = BinaryPrimitives.ReadUInt32LittleEndian(data[8..12]);
            byte bpp = data[12];
            byte numBlocks = data[13];
            byte texMode = data[14];
            byte alpha = data[15];
            uint dataOffset = BinaryPrimitives.ReadUInt32LittleEndian(data[16..20]);

            //Its unlikely that any mip textures are ever gonna be this big
            if (width > 512 || height > 512) return false;

            //We only support MIP files which have a bpp of 4 and 8
            if (bpp != 8 && bpp != 4) return false;

            //Alpha can only be 0 or 1
            if (alpha > 1) return false;

            //If the data offset is past the end of the file, its not a MIP
            if (dataOffset > data.Length || clutOffset > data.Length) return false;

            //If the size of the image is too big for the data passed in, its not a MIP
            if (width * height * bpp / 8 > data.Length - dataOffset) return false;
        }
        catch
        {
            //If the data is invalid an invalid encrypted file, its not a MIP
            return false;
        }
        
        return true;
    }

    protected GameAssetType DetermineAssetType(Span<byte> data, TokenPlatform? tokenPlatform)
    {
        // LBP assets
        if (MatchesMagic(data, "TEX "u8)) return GameAssetType.Texture;
        if (MatchesMagic(data, "GTF "u8)) return GameAssetType.GameDataTexture;
        if (MatchesMagic(data, "MATb"u8)) return GameAssetType.Material;
        if (MatchesMagic(data, "PLNb"u8)) return GameAssetType.Plan;
        if (MatchesMagic(data, "LVLb"u8)) return GameAssetType.Level;
        if (MatchesMagic(data, "GMTb"u8)) return GameAssetType.GfxMaterial;
        if (MatchesMagic(data, "MSHb"u8)) return GameAssetType.Mesh;
        if (MatchesMagic(data, "PALb"u8)) return GameAssetType.Palette;
        if (MatchesMagic(data, "FSHb"u8)) return GameAssetType.Script;
        if (MatchesMagic(data, "RECb"u8)) return GameAssetType.MoveRecording;
        if (MatchesMagic(data, "VOPb"u8)) return GameAssetType.VoiceRecording;
        if (MatchesMagic(data, "PTGb"u8)) return GameAssetType.Painting;
        if (MatchesMagic(data, "PRFb"u8)) return GameAssetType.SyncedProfile;
        if (MatchesMagic(data, "MATT"u8)) return GameAssetType.GriefSongState;
        if (MatchesMagic(data, "SSPt"u8)) return GameAssetType.SoftPhysicsSettings;
        if (MatchesMagic(data, "BEVb"u8)) return GameAssetType.Bevel;
        
        // Traditional files
        // Good reference for magics: https://en.wikipedia.org/wiki/List_of_file_signatures
        if (MatchesMagic(data, 0xFFD8FFE0)) return GameAssetType.Jpeg;
        if (MatchesMagic(data, 0x89504E470D0A1A0A)) return GameAssetType.Png;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (tokenPlatform is null or TokenPlatform.PSP && IsPspTga(data)) return GameAssetType.Tga;
        if (tokenPlatform is null or TokenPlatform.PSP && this.IsMip(data)) return GameAssetType.Mip;
                    
        this.Warn($"Unknown asset header [0x{Convert.ToHexString(data[..4])}] [str: {Encoding.ASCII.GetString(data[..4])}]");

        return GameAssetType.Unknown;
    }
}