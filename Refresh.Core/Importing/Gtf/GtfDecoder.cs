using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;

namespace Refresh.GameServer.Importing.Gtf;

public class GtfDecoder : ImageDecoder
{
    protected override Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        Span<byte> magic = stackalloc byte[4];
        
        stream.ReadExactly(magic);
        
        //Check magic
        if(!magic.SequenceEqual("GTF "u8)) ThrowInvalidImageContentException();

        //Read the header in
        GtfHeader header = GtfHeader.Read(stream);

        //If we dont know what format this is, throw an error
        if(!Enum.IsDefined(header.PixelFormat)) ThrowInvalidImageContentException();

        Image<TPixel> image = new(options.Configuration, header.Width, header.Height);
        Buffer2D<TPixel> pixels = image.Frames.RootFrame.PixelBuffer;

        this.ProcessPixels(stream, pixels, header);

        return image;
    }
    
    private void ProcessPixels<TPixel>(Stream compressedStream, Buffer2D<TPixel> pixels, GtfHeader header) where TPixel : unmanaged, IPixel<TPixel>
    {
        TexStream decompressedStream = new(compressedStream, false);
        BEBinaryReader reader = new(decompressedStream);

        if (header.PixelFormat is GtfPixelFormat.CompressedDxt1 or GtfPixelFormat.CompressedDxt23 or GtfPixelFormat.CompressedDxt45)
        {
            BcDecoder decoder = new();

            CompressionFormat format = header.PixelFormat switch {
                GtfPixelFormat.CompressedDxt1 => CompressionFormat.Bc1,
                GtfPixelFormat.CompressedDxt23 => CompressionFormat.Bc2,
                GtfPixelFormat.CompressedDxt45 => CompressionFormat.Bc3,
                _ => throw new ArgumentOutOfRangeException(),
            };
            
            // im not the happiest with this massive allocation :/
            Span<ColorRgba32> colors = decoder.DecodeRaw(decompressedStream, pixels.Width, pixels.Height, format).AsSpan();
   
            for (int y = 0; y < pixels.Height; y++)
            {
                Span<TPixel> row = pixels.DangerousGetRowSpan(y);
                if (typeof(TPixel) == typeof(Rgba32))
                {
                    Span<ColorRgba32> rgba32Row = row.Cast<TPixel, ColorRgba32>();
                    colors.Slice(y * header.Width, header.Width).CopyTo(rgba32Row);
                }
                else
                {
                    for (int x = 0; x < pixels.Width; x++)
                    {
                        TPixel pixel = new();
                        pixel.FromRgba32(Unsafe.BitCast<ColorRgba32, Rgba32>(colors[y * header.Width + x]));
                        row[x] = pixel;
                    }
                }
            }
        }
        else
        {
            for (int y = 0; y < pixels.Height; y++)
            {
                Span<TPixel> row = pixels.DangerousGetRowSpan(y);

                for (int x = 0; x < row.Length; x++)
                {
                    TPixel pixel = new();
                    switch (header.PixelFormat)
                    {

                        case GtfPixelFormat.B8:
                            pixel.FromRgba32(new Rgba32(0, 0, reader.ReadByte(), 255));
                            break;
                        case GtfPixelFormat.A1R5G5B5: {
                            ushort packed = reader.ReadUInt16();
                            byte a = ShiftLeftShiftingInLsb((byte)((packed & 0b1000000000000000) >> 15), 7);
                            byte r = ShiftLeftShiftingInLsb((byte)((packed & 0b0111110000000000) >> 10), 3);
                            byte g = ShiftLeftShiftingInLsb((byte)((packed & 0b0000001111100000) >> 5), 3);
                            byte b = ShiftLeftShiftingInLsb((byte)(packed & 0b0000000000011111), 3);
                            pixel.FromRgba32(new Rgba32(r, g, b, a));
                            break;
                        }
                        case GtfPixelFormat.A4R4G4B4: {
                            ushort packed = reader.ReadUInt16();
                            byte a = ShiftLeftShiftingInLsb((byte)((packed & 0b1111000000000000) >> 12), 4);
                            byte r = ShiftLeftShiftingInLsb((byte)((packed & 0b0000111100000000) >> 8), 4);
                            byte g = ShiftLeftShiftingInLsb((byte)((packed & 0b0000000011110000) >> 4), 4);
                            byte b = ShiftLeftShiftingInLsb((byte)(packed & 0b0000000000001111), 4);
                            pixel.FromRgba32(new Rgba32(r, g, b, a));
                            break;
                        }
                        case GtfPixelFormat.R5G6B5: {
                            ushort packed = reader.ReadUInt16();
                            byte r = ShiftLeftShiftingInLsb((byte)((packed & 0b1111100000000000) >> 11), 3);
                            byte g = ShiftLeftShiftingInLsb((byte)((packed & 0b0000011111100000) >> 5), 2);
                            byte b = ShiftLeftShiftingInLsb((byte)(packed & 0b0000000000011111), 3);
                            pixel.FromRgba32(new Rgba32(r, g, b, 255));
                            break;
                        }
                        case GtfPixelFormat.A8R8G8B8: {
                            byte a = reader.ReadByte();
                            byte r = reader.ReadByte();
                            byte b = reader.ReadByte();
                            byte g = reader.ReadByte();
                            pixel.FromRgba32(new Rgba32(r, g, b, a));
                            break;
                        }
                        case GtfPixelFormat.G8B8:
                            pixel.FromRgba32(new Rgba32(0, reader.ReadByte(), reader.ReadByte(), 255)); 
                            break;
                        case GtfPixelFormat.R6G5B5: {
                            ushort packed = reader.ReadUInt16();
                            byte r = ShiftLeftShiftingInLsb((byte)((packed & 0b1111110000000000) >> 10), 2);
                            byte g = ShiftLeftShiftingInLsb((byte)((packed & 0b0000001111100000) >> 5), 3);
                            byte b = ShiftLeftShiftingInLsb((byte)(packed & 0b0000000000011111), 3);
                            pixel.FromRgba32(new Rgba32(r, g, b, 255));
                            break;
                        }
                        case GtfPixelFormat.R5G5B5A1: {
                            ushort packed = reader.ReadUInt16();
                            byte r = ShiftLeftShiftingInLsb((byte)((packed & 0b1111100000000000) >> 11), 3);
                            byte g = ShiftLeftShiftingInLsb((byte)((packed & 0b0000011111000000) >> 6), 3);
                            byte b = ShiftLeftShiftingInLsb((byte)((packed & 0b0000000000111110) >> 1), 3);
                            byte a = ShiftLeftShiftingInLsb((byte)(packed & 0b0000000000000001), 7);
                            pixel.FromRgba32(new Rgba32(r, g, b, a));
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    row[x] = pixel;
                }
            }
        }
    }

    /// <summary>
    /// Shifts a number to the left, shifting the LSB in
    /// </summary>
    /// <param name="num">The number to shift</param>
    /// <param name="amt">The amount of bits to shift</param>
    /// <returns>The shifted number</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static byte ShiftLeftShiftingInLsb(byte num, byte amt)
    {
        int mask = (1 << amt) - 1;
        return (byte)((num << amt) | ((num & 1) * mask));
    }
    
    protected override Image Decode(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        return this.Decode<Rgba32>(options, stream, cancellationToken);
    }
    
    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        Span<byte> magic = stackalloc byte[4];
        
        stream.ReadExactly(magic);
        
        //Check magic
        if(!magic.SequenceEqual("GTF "u8)) ThrowInvalidImageContentException();

        //Read the header in
        GtfHeader header = GtfHeader.Read(stream);

        //If we dont know what format this is, throw an error
        if(!Enum.IsDefined(header.PixelFormat)) ThrowInvalidImageContentException();

        return new ImageInfo(
            new PixelTypeInfo(header.PixelFormat.BitsPerPixel()),
            new Size(header.Width, header.Height),
            new ImageMetadata
            {
                HorizontalResolution = header.Width,
                VerticalResolution = header.Height,
                ResolutionUnits = PixelResolutionUnit.AspectRatio,
            }
        );
    }
    
    [DoesNotReturn]
    private static void ThrowInvalidImageContentException()
        => throw new InvalidImageContentException("The image is not a valid GTF image.");
}